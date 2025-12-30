using WattsTap.Core.React;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class BugReportUIModel : UIBaseModel
    {
        public ReactiveProperty<string> UserDescription { get; private set; }
        public ReactiveProperty<bool> IsLoading { get; private set; }
        public ReactiveProperty<bool> IsSendEnabled { get; private set; }
        public ReactiveProperty<string> DeviceInfo { get; private set; }
        public ReactiveProperty<string> StatusMessage { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            
            UserDescription = new ReactiveProperty<string>(string.Empty);
            IsLoading = new ReactiveProperty<bool>(false);
            IsSendEnabled = new ReactiveProperty<bool>(true);
            DeviceInfo = new ReactiveProperty<string>(string.Empty);
            StatusMessage = new ReactiveProperty<string>(string.Empty);
            
            // Update send button state based on description
            UserDescription.OnValueChanged += OnDescriptionChanged;
        }

        private void OnDescriptionChanged(string description)
        {
            // Enable send button only if description is not empty
            IsSendEnabled.Value = !IsLoading.Value && !string.IsNullOrWhiteSpace(description);
        }

        public void SetLoading(bool isLoading)
        {
            IsLoading.Value = isLoading;
            IsSendEnabled.Value = !isLoading && !string.IsNullOrWhiteSpace(UserDescription.Value);
        }

        public override void Dispose()
        {
            UserDescription?.Dispose();
            IsLoading?.Dispose();
            IsSendEnabled?.Dispose();
            DeviceInfo?.Dispose();
            StatusMessage?.Dispose();
            
            base.Dispose();
        }
    }
}

