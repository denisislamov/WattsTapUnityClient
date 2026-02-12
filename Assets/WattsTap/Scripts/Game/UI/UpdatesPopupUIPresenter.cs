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
        private IUpgradesService _upgradesService;

        protected override void OnInit()
        {
            UnityEngine.Debug.Log("[UpdatesPopupUIPresenter] OnInit called");
            
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);
            ServiceLocator.TryGet(out _upgradesService);

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
            
            // Load and display upgrades data
            LoadUpgradesData();
        }
        
        /// <summary>
        /// Load upgrades data from service and populate the list
        /// </summary>
        private void LoadUpgradesData()
        {
            UnityEngine.Debug.Log($"[UpdatesPopupUIPresenter] LoadUpgradesData called. _upgradesService is {(_upgradesService != null ? "NOT null" : "null")}");
            
            if (_upgradesService == null)
            {
                UnityEngine.Debug.LogWarning("[UpdatesPopupUIPresenter] UpgradesService not found");
                return;
            }
            
            var upgradesData = _upgradesService.GetAllUpgradesForDisplay();
            UnityEngine.Debug.Log($"[UpdatesPopupUIPresenter] Got {upgradesData?.Count ?? 0} upgrades from service");
            
            View.PopulateUpgradesList(upgradesData);
            UnityEngine.Debug.Log($"[UpdatesPopupUIPresenter] PopulateUpgradesList called");
            
            // Store in model for potential future use
            Model.SetUpgradesData(upgradesData);
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

