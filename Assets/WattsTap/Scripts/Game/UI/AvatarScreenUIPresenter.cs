using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class AvatarScreenUIPresenter : UIBasePresenter<AvatarScreenUIView, AvatarScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _hapticService);
            ServiceLocator.TryGet(out _uiService);

            if (View.BackButton != null)
            {
                View.BackButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnBackClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.AvatarScreen);
        }

        protected override void OnDispose()
        {
            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackClicked);
            }
        }
    }
}

