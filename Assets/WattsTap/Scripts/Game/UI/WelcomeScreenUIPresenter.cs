using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class WelcomeScreenUIPresenter : UIBasePresenter<WelcomeScreenUIView, WelcomeScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _hapticService);

            if (View.StartButton != null)
            {
                View.StartButton.onClick.AddListener(OnStartClicked);
            }
        }

        private void OnStartClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.WelcomeScreen);
        }

        protected override void OnDispose()
        {
            if (View?.StartButton != null)
            {
                View.StartButton.onClick.RemoveListener(OnStartClicked);
            }
        }
    }
}
