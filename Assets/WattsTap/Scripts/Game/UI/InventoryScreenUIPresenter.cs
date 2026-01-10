using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIPresenter : UIBasePresenter<InventoryScreenUIView, InventoryScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            // Subscribe to model changes
            Model.HitsCurrent.OnValueChanged += OnHitsChanged;
            Model.HitsMax.OnValueChanged += OnHitsChanged;
            Model.CoinsPerTap.OnValueChanged += OnCoinsPerTapChanged;
            
            // Initialize view with current values
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
            View.UpdateCoinsPerTap(Model.CoinsPerTap.Value);

            // Navigation buttons
            if (View.MiningButton != null)
            {
                View.MiningButton.onClick.AddListener(OnMiningButtonClicked);
            }

            if (View.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.AddListener(OnFriendsReferralButtonClicked);
            }

            if (View.QuestsButton != null)
            {
                View.QuestsButton.onClick.AddListener(OnQuestsButtonClicked);
            }
        }

        #region Event Handlers - Model
        
        private void OnHitsChanged(int value)
        {
            View.UpdateHits(Model.HitsCurrent.Value, Model.HitsMax.Value);
        }

        private void OnCoinsPerTapChanged(int value)
        {
            View.UpdateCoinsPerTap(value);
        }
        
        #endregion

        #region Event Handlers - Buttons

        private void OnMiningButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen to return to MainMenu
            _uiService?.Close(UIConstants.InventoryScreen);
        }

        private void OnFriendsReferralButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen and open FriendsReferralScreen
            _uiService?.Close(UIConstants.InventoryScreen);
            _uiService?.Open(UIConstants.FriendsReferralScreen);
        }

        private void OnQuestsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Close InventoryScreen and open QuestsScreen
            _uiService?.Close(UIConstants.InventoryScreen);
            _uiService?.Open(UIConstants.QuestsScreen);
        }

        #endregion

        protected override void OnDispose()
        {
            // Unsubscribe from model changes
            if (Model != null)
            {
                Model.HitsCurrent.OnValueChanged -= OnHitsChanged;
                Model.HitsMax.OnValueChanged -= OnHitsChanged;
                Model.CoinsPerTap.OnValueChanged -= OnCoinsPerTapChanged;
            }
            
            // Remove button listeners
            if (View?.MiningButton != null)
            {
                View.MiningButton.onClick.RemoveListener(OnMiningButtonClicked);
            }

            if (View?.FriendsReferralButton != null)
            {
                View.FriendsReferralButton.onClick.RemoveListener(OnFriendsReferralButtonClicked);
            }

            if (View?.QuestsButton != null)
            {
                View.QuestsButton.onClick.RemoveListener(OnQuestsButtonClicked);
            }
        }
    }
}

