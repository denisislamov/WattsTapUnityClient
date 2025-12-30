using System;

namespace WattsTap.Core.Services.BugReport
{
    /// <summary>
    /// Device and system information for bug reports
    /// </summary>
    [Serializable]
    public class BugReportData
    {
        // User Input
        public string Description;
        
        // Device Info
        public string DeviceModel;
        public string OperatingSystem;
        public string SystemLanguage;
        public string DeviceType;
        public int SystemMemoryMB;
        public int GraphicsMemoryMB;
        public string GraphicsDeviceName;
        public string GraphicsDeviceVendor;
        
        // Screen Info
        public int ScreenWidth;
        public int ScreenHeight;
        public float ScreenDPI;
        public string ScreenOrientation;
        
        // Browser/WebGL Info
        public string BrowserUserAgent;
        public string BrowserLanguage;
        public string CurrentURL;
        public bool IsOnline;
        public bool CookiesEnabled;
        public bool TouchSupport;
        public float DevicePixelRatio;
        public string Timezone;
        public string WebGLRenderer;
        public string WebGLVendor;
        
        // Application Info
        public string Platform;
        public string UnityVersion;
        public string AppVersion;
        public string Timestamp;
        
        // Performance Info
        public int JSMemoryUsedMB;
        public int JSMemoryTotalMB;
        
        // Logs
        public string RecentLogs;
    }

    /// <summary>
    /// Interface for bug report service
    /// </summary>
    public interface IBugReportService : IService
    {
        /// <summary>
        /// Email address to send bug reports to
        /// </summary>
        string TargetEmail { get; set; }
        
        /// <summary>
        /// Fired when bug report is successfully sent
        /// </summary>
        event Action OnReportSent;
        
        /// <summary>
        /// Fired when bug report sending fails
        /// </summary>
        event Action<string> OnReportFailed;
        
        /// <summary>
        /// Collects current device information
        /// </summary>
        BugReportData CollectDeviceInfo();
        
        /// <summary>
        /// Sends bug report with user description
        /// </summary>
        /// <param name="userDescription">User's description of the bug</param>
        void SendReport(string userDescription);
        
        /// <summary>
        /// Opens bug report UI
        /// </summary>
        void ShowBugReportUI();
        
        /// <summary>
        /// Closes bug report UI
        /// </summary>
        void CloseBugReportUI();
        
        /// <summary>
        /// Adds a log entry to be included in the report
        /// </summary>
        void CaptureLog(string log, string stackTrace, UnityEngine.LogType logType);
    }
}

