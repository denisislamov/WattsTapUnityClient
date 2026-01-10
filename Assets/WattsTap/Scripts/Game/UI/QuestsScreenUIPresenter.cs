using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class QuestsScreenUIPresenter : UIBasePresenter<QuestsScreenUIView, QuestsScreenUIModel>
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
            
            // Load quests data
            LoadData();
        }
        
        private void LoadData()
        {
            // TODO: Load quests from service when implemented
            // For now, show placeholder
            View.SetNoQuestsPlaceholderActive(true);
        }

        #region Event Handlers - Buttons

        private void OnMainMenuButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Simply close the QuestsScreen to return to MainMenu
            _uiService?.Close(UIConstants.QuestsScreen);
        }

        private void OnFriendsReferralButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close QuestsScreen and open FriendsReferralScreen
            _uiService?.Close(UIConstants.QuestsScreen);
            _uiService?.Open(UIConstants.FriendsReferralScreen);
        }

        private void OnInventoryButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close QuestsScreen and open InventoryScreen
            _uiService?.Close(UIConstants.QuestsScreen);
            _uiService?.Open(UIConstants.InventoryScreen);
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
        }
    }
}

