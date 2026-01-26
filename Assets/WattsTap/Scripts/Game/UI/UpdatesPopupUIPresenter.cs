using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class UpdatesPopupUIPresenter : UIBasePresenter<UpdatesPopupUIView, UpdatesPopupUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            // Navigation buttons
            if (View.MainMenuButton != null)
            {
                View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            }
        }

        #region Event Handlers - Buttons

        private void OnMainMenuButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Simply close the UpdatesPopup to return to MainMenu
            _uiService?.Close(UIConstants.UpdatesPopup);
        }

        #endregion

        protected override void OnDispose()
        {
            // Remove button listeners
            if (View?.MainMenuButton != null)
            {
                View.MainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
            }
        }
    }
}

