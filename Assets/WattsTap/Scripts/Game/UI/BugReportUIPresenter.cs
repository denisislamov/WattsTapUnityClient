using System.Text;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Services.BugReport;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class BugReportUIPresenter : UIBasePresenter<BugReportUIView, BugReportUIModel>
    {
        private IBugReportService _bugReportService;
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            // Get services
            ServiceLocator.TryGet(out _bugReportService);
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            // Subscribe to model changes
            Model.IsSendEnabled.OnValueChanged += OnSendEnabledChanged;
            Model.IsLoading.OnValueChanged += OnLoadingChanged;
            Model.StatusMessage.OnValueChanged += OnStatusMessageChanged;

            // Subscribe to service events
            if (_bugReportService != null)
            {
                _bugReportService.OnReportSent += OnReportSent;
                _bugReportService.OnReportFailed += OnReportFailed;
                
                // Collect and display device info preview
                var deviceInfo = _bugReportService.CollectDeviceInfo();
                Model.DeviceInfo.Value = FormatDeviceInfoPreview(deviceInfo);
            }

            // Subscribe to UI events
            if (View.CloseButton != null)
            {
                View.CloseButton.onClick.AddListener(OnCloseClicked);
            }

            if (View.SendButton != null)
            {
                View.SendButton.onClick.AddListener(OnSendClicked);
            }

            if (View.DescriptionInput != null)
            {
                View.DescriptionInput.onValueChanged.AddListener(OnDescriptionChanged);
            }

            // Initialize UI state
            View.SetSendButtonInteractable(false);
            View.SetLoading(false);
            View.UpdateDeviceInfo(Model.DeviceInfo.Value);
        }

        private void OnCloseClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.BugReportScreen);
        }

        private void OnSendClicked()
        {
            _hapticService?.ButtonPressed();

            string description = Model.UserDescription.Value;
            
            // if (string.IsNullOrWhiteSpace(description))
            // {
            //     Model.StatusMessage.Value = "Please describe the issue";
            //     return;
            // }

            Model.SetLoading(true);
            Model.StatusMessage.Value = "Preparing report...";

            _bugReportService?.SendReport(description);
        }

        private void OnDescriptionChanged(string newValue)
        {
            Model.UserDescription.Value = newValue;
        }

        private void OnSendEnabledChanged(bool isEnabled)
        {
            View.SetSendButtonInteractable(isEnabled);
        }

        private void OnLoadingChanged(bool isLoading)
        {
            View.SetLoading(isLoading);
        }

        private void OnStatusMessageChanged(string message)
        {
            View.UpdateStatusMessage(message);
        }

        private void OnReportSent()
        {
            Model.SetLoading(false);
            Model.StatusMessage.Value = "Report sent successfully!";
            
            // _hapticService?.SuccessNotification();
            
            // Clear description for next report
            Model.UserDescription.Value = string.Empty;
            View.ClearDescription();
        }

        private void OnReportFailed(string error)
        {
            Model.SetLoading(false);
            Model.StatusMessage.Value = $"Failed to send: {error}";
            
            // _hapticService?.ErrorNotification();
        }

        private string FormatDeviceInfoPreview(BugReportData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Device: {data.DeviceModel}");
            sb.AppendLine($"OS: {data.OperatingSystem}");
            sb.AppendLine($"Screen: {data.ScreenWidth}x{data.ScreenHeight}");
            sb.AppendLine($"Memory: {data.SystemMemoryMB} MB");
            sb.AppendLine($"Graphics: {data.GraphicsDeviceName}");
            return sb.ToString();
        }

        protected override void OnDispose()
        {
            // Unsubscribe from model
            if (Model != null)
            {
                Model.IsSendEnabled.OnValueChanged -= OnSendEnabledChanged;
                Model.IsLoading.OnValueChanged -= OnLoadingChanged;
                Model.StatusMessage.OnValueChanged -= OnStatusMessageChanged;
            }

            // Unsubscribe from service
            if (_bugReportService != null)
            {
                _bugReportService.OnReportSent -= OnReportSent;
                _bugReportService.OnReportFailed -= OnReportFailed;
            }

            // Unsubscribe from UI
            if (View != null)
            {
                if (View.CloseButton != null)
                {
                    View.CloseButton.onClick.RemoveListener(OnCloseClicked);
                }

                if (View.SendButton != null)
                {
                    View.SendButton.onClick.RemoveListener(OnSendClicked);
                }

                if (View.DescriptionInput != null)
                {
                    View.DescriptionInput.onValueChanged.RemoveListener(OnDescriptionChanged);
                }
            }
        }
    }
}

