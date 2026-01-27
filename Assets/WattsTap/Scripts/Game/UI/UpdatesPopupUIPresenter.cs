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

            if (View.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.AddListener(OnFriendsReferralButtonClicked);
            }

            if (View.InventoryButton != null)
            {
                View.InventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }

            if (View.QuestsButton != null)
            {
                View.QuestsButton.onClick.AddListener(OnQuestsButtonClicked);
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

        private void OnFriendsReferralButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close UpdatesPopup and open FriendsReferralScreen
            _uiService?.Close(UIConstants.UpdatesPopup);
            _uiService?.Open(UIConstants.FriendsReferralScreen);
        }

        private void OnInventoryButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close UpdatesPopup and open InventoryScreen
            _uiService?.Close(UIConstants.UpdatesPopup);
            _uiService?.Open(UIConstants.InventoryScreen);
        }

        private void OnQuestsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close UpdatesPopup and open QuestsScreen
            _uiService?.Close(UIConstants.UpdatesPopup);
            _uiService?.Open(UIConstants.QuestsScreen);
        }

        #endregion

        protected override void OnDispose()
        {
            // Remove button listeners
            if (View?.MainMenuButton != null)
            {
                View.MainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
            }

            if (View?.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.RemoveListener(OnFriendsReferralButtonClicked);
            }

            if (View?.InventoryButton != null)
            {
                View.InventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
            }

            if (View?.QuestsButton != null)
            {
                View.QuestsButton.onClick.RemoveListener(OnQuestsButtonClicked);
            }
        }
    }
}

