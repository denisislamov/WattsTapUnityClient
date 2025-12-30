using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.UI;

namespace WattsTap.Core.Services.BugReport
{
    /// <summary>
    /// Service for collecting and sending bug reports
    /// Works in WebGL environment
    /// </summary>
    public class BugReportService : IBugReportService
    {
        #region JavaScript Interop

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenMailClient(string email, string subject, string body);

        [DllImport("__Internal")]
        private static extern string GetBrowserUserAgent();

        [DllImport("__Internal")]
        private static extern void ShowBugReportNotification(string message, bool isSuccess);

        [DllImport("__Internal")]
        private static extern string GetBrowserLanguage();

        [DllImport("__Internal")]
        private static extern string GetScreenOrientation();

        [DllImport("__Internal")]
        private static extern string GetWebGLInfo();

        [DllImport("__Internal")]
        private static extern string GetCurrentURL();

        [DllImport("__Internal")]
        private static extern string GetPerformanceInfo();
#endif

        #endregion

        #region Constants

        private const int MAX_LOG_ENTRIES = 100;
        private const string DEFAULT_EMAIL = "islamov.denis@wattstap.energy";

        #endregion

        #region Private Fields

        private readonly Queue<string> _recentLogs = new Queue<string>();
        private IUIService _uiService;

        #endregion

        #region IService Implementation

        public int InitializationOrder => 100;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;

            TargetEmail = DEFAULT_EMAIL;
            
            // Subscribe to Unity logs
            Application.logMessageReceived += OnLogMessageReceived;
            
            IsInitialized = true;
            Debug.Log("[BugReportService] Initialized");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;

            Application.logMessageReceived -= OnLogMessageReceived;
            _recentLogs.Clear();
            
            IsInitialized = false;
        }

        #endregion

        #region IBugReportService Implementation

        public string TargetEmail { get; set; }

        public event Action OnReportSent;
        public event Action<string> OnReportFailed;

        public BugReportData CollectDeviceInfo()
        {
            var data = new BugReportData
            {
                // Device Info
                DeviceModel = SystemInfo.deviceModel,
                OperatingSystem = SystemInfo.operatingSystem,
                SystemLanguage = Application.systemLanguage.ToString(),
                DeviceType = SystemInfo.deviceType.ToString(),
                SystemMemoryMB = SystemInfo.systemMemorySize,
                GraphicsMemoryMB = SystemInfo.graphicsMemorySize,
                GraphicsDeviceName = SystemInfo.graphicsDeviceName,
                GraphicsDeviceVendor = SystemInfo.graphicsDeviceVendor,
                
                // Screen Info
                ScreenWidth = Screen.width,
                ScreenHeight = Screen.height,
                ScreenDPI = Screen.dpi,
                ScreenOrientation = GetScreenOrientationString(),
                
                // Application Info
                Platform = Application.platform.ToString(),
                UnityVersion = Application.unityVersion,
                AppVersion = Application.version,
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                
                // Browser/WebGL Info
                BrowserUserAgent = GetUserAgent(),
                BrowserLanguage = GetBrowserLanguageString(),
                CurrentURL = GetCurrentURLString(),
                IsOnline = Application.internetReachability != NetworkReachability.NotReachable,
                
                // Logs
                RecentLogs = GetRecentLogsFormatted()
            };

            // Get additional WebGL info
            PopulateWebGLInfo(data);
            PopulatePerformanceInfo(data);

            return data;
        }

        public void SendReport(string userDescription)
        {
            try
            {
                var data = CollectDeviceInfo();
                data.Description = userDescription;

                string subject = $"[WattsTap Bug Report] {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
                string body = FormatReportBody(data);

#if UNITY_WEBGL && !UNITY_EDITOR
                OpenMailClient(TargetEmail, subject, body);
                ShowBugReportNotification("Bug report opened in mail client", true);
#else
                // For Editor testing - log to console
                Debug.Log($"[BugReportService] Would send email to: {TargetEmail}");
                Debug.Log($"[BugReportService] Subject: {subject}");
                Debug.Log($"[BugReportService] Body:\n{body}");
                
                // Try to open mailto link in Editor
                string mailtoUrl = $"mailto:{TargetEmail}?subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
                Application.OpenURL(mailtoUrl);
#endif

                OnReportSent?.Invoke();
                CloseBugReportUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[BugReportService] Failed to send report: {ex.Message}");
                OnReportFailed?.Invoke(ex.Message);
                
#if UNITY_WEBGL && !UNITY_EDITOR
                ShowBugReportNotification("Failed to send bug report", false);
#endif
            }
        }

        public void ShowBugReportUI()
        {
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Open(UIConstants.BugReportScreen, _uiService.OverlayRoot);
        }

        public void CloseBugReportUI()
        {
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.BugReportScreen);
        }

        public void CaptureLog(string log, string stackTrace, LogType logType)
        {
            string formattedLog = $"[{DateTime.Now:HH:mm:ss}] [{logType}] {log}";
            
            if (logType == LogType.Error || logType == LogType.Exception)
            {
                formattedLog += $"\n  Stack: {stackTrace}";
            }

            AddLogEntry(formattedLog);
        }

        #endregion

        #region Private Methods

        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            // Only capture warnings, errors and exceptions
            if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception)
            {
                CaptureLog(logString, stackTrace, type);
            }
        }

        private void AddLogEntry(string entry)
        {
            _recentLogs.Enqueue(entry);
            
            while (_recentLogs.Count > MAX_LOG_ENTRIES)
            {
                _recentLogs.Dequeue();
            }
        }

        private string GetRecentLogsFormatted()
        {
            if (_recentLogs.Count == 0)
            {
                return "No recent logs captured";
            }

            var sb = new StringBuilder();
            foreach (var log in _recentLogs)
            {
                sb.AppendLine(log);
            }
            return sb.ToString();
        }

        private string GetUserAgent()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return GetBrowserUserAgent();
            }
            catch
            {
                return "Unknown";
            }
#else
            return "Editor/Standalone";
#endif
        }

        private string FormatReportBody(BugReportData data)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("=== BUG REPORT ===");
            sb.AppendLine();
            
            sb.AppendLine("--- USER DESCRIPTION ---");
            sb.AppendLine(string.IsNullOrEmpty(data.Description) ? "No description provided" : data.Description);
            sb.AppendLine();
            
            sb.AppendLine("--- DEVICE INFO ---");
            sb.AppendLine($"Device Model: {data.DeviceModel}");
            sb.AppendLine($"Device Type: {data.DeviceType}");
            sb.AppendLine($"OS: {data.OperatingSystem}");
            sb.AppendLine($"System Language: {data.SystemLanguage}");
            sb.AppendLine($"Platform: {data.Platform}");
            sb.AppendLine($"Online: {data.IsOnline}");
            sb.AppendLine();
            
            sb.AppendLine("--- HARDWARE ---");
            sb.AppendLine($"System Memory: {data.SystemMemoryMB} MB");
            sb.AppendLine($"Graphics Memory: {data.GraphicsMemoryMB} MB");
            sb.AppendLine($"Graphics Device: {data.GraphicsDeviceName}");
            sb.AppendLine($"Graphics Vendor: {data.GraphicsDeviceVendor}");
            sb.AppendLine();
            
            sb.AppendLine("--- SCREEN ---");
            sb.AppendLine($"Resolution: {data.ScreenWidth}x{data.ScreenHeight}");
            sb.AppendLine($"DPI: {data.ScreenDPI}");
            sb.AppendLine($"Orientation: {data.ScreenOrientation}");
            sb.AppendLine($"Device Pixel Ratio: {data.DevicePixelRatio}");
            sb.AppendLine();
            
            sb.AppendLine("--- APPLICATION ---");
            sb.AppendLine($"App Version: {data.AppVersion}");
            sb.AppendLine($"Unity Version: {data.UnityVersion}");
            sb.AppendLine($"Timestamp: {data.Timestamp}");
            sb.AppendLine();
            
            sb.AppendLine("--- BROWSER/WEBGL ---");
            sb.AppendLine($"User Agent: {data.BrowserUserAgent}");
            sb.AppendLine($"Browser Language: {data.BrowserLanguage}");
            sb.AppendLine($"Current URL: {data.CurrentURL}");
            sb.AppendLine($"Touch Support: {data.TouchSupport}");
            sb.AppendLine($"Cookies Enabled: {data.CookiesEnabled}");
            sb.AppendLine($"Timezone: {data.Timezone}");
            sb.AppendLine($"WebGL Renderer: {data.WebGLRenderer}");
            sb.AppendLine($"WebGL Vendor: {data.WebGLVendor}");
            sb.AppendLine();
            
            sb.AppendLine("--- PERFORMANCE ---");
            sb.AppendLine($"JS Heap Used: {data.JSMemoryUsedMB} MB");
            sb.AppendLine($"JS Heap Total: {data.JSMemoryTotalMB} MB");
            sb.AppendLine();
            
            sb.AppendLine("--- RECENT LOGS ---");
            sb.AppendLine(data.RecentLogs);
            
            sb.AppendLine("=== END OF REPORT ===");
            
            return sb.ToString();
        }

        private string GetScreenOrientationString()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return GetScreenOrientation();
            }
            catch
            {
                return Screen.width > Screen.height ? "landscape" : "portrait";
            }
#else
            return Screen.width > Screen.height ? "landscape" : "portrait";
#endif
        }

        private string GetBrowserLanguageString()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return GetBrowserLanguage();
            }
            catch
            {
                return "Unknown";
            }
#else
            return Application.systemLanguage.ToString();
#endif
        }

        private string GetCurrentURLString()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                return GetCurrentURL();
            }
            catch
            {
                return "Unknown";
            }
#else
            return "Editor/Standalone";
#endif
        }

        private void PopulateWebGLInfo(BugReportData data)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string jsonInfo = GetWebGLInfo();
                if (!string.IsNullOrEmpty(jsonInfo))
                {
                    var info = JsonUtility.FromJson<WebGLInfoData>(jsonInfo);
                    if (info != null)
                    {
                        data.CookiesEnabled = info.cookiesEnabled;
                        data.TouchSupport = info.touchSupport;
                        data.DevicePixelRatio = info.devicePixelRatio;
                        data.Timezone = info.timezone;
                        data.WebGLRenderer = info.webglRenderer;
                        data.WebGLVendor = info.webglVendor;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BugReportService] Failed to get WebGL info: {ex.Message}");
            }
#else
            data.CookiesEnabled = false;
            data.TouchSupport = Input.touchSupported;
            data.DevicePixelRatio = 1f;
            data.Timezone = TimeZoneInfo.Local.Id;
            data.WebGLRenderer = SystemInfo.graphicsDeviceName;
            data.WebGLVendor = SystemInfo.graphicsDeviceVendor;
#endif
        }

        private void PopulatePerformanceInfo(BugReportData data)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                string jsonInfo = GetPerformanceInfo();
                if (!string.IsNullOrEmpty(jsonInfo))
                {
                    var info = JsonUtility.FromJson<PerformanceInfoData>(jsonInfo);
                    if (info != null)
                    {
                        data.JSMemoryUsedMB = info.memoryUsed;
                        data.JSMemoryTotalMB = info.memoryTotal;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BugReportService] Failed to get performance info: {ex.Message}");
            }
#else
            data.JSMemoryUsedMB = 0;
            data.JSMemoryTotalMB = 0;
#endif
        }

        #endregion

        #region Helper Classes

        [Serializable]
        private class WebGLInfoData
        {
            public string platform;
            public bool cookiesEnabled;
            public bool onLine;
            public bool touchSupport;
            public int screenWidth;
            public int screenHeight;
            public float devicePixelRatio;
            public string timezone;
            public string webglRenderer;
            public string webglVendor;
        }

        [Serializable]
        private class PerformanceInfoData
        {
            public int memoryUsed;
            public int memoryTotal;
            public int fps;
        }

        #endregion
    }
}

