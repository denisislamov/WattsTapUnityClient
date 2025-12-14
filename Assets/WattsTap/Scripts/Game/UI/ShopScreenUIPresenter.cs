using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopScreenUIPresenter : UIBasePresenter<ShopScreenUIView, ShopScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            if (View.ChestsButton != null)
            {
                View.ChestsButton.onClick.AddListener(OnChestsButtonClicked);
            }

            if (View.BoostersButton != null)
            {
                View.BoostersButton.onClick.AddListener(OnBoostersButtonClicked);
            }

            foreach (var closeButton in View.CloseButtons)
            {
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // Subscribe to chest item buttons
            if (View.ChestItemOpenButtons != null)
            {
                for (int i = 0; i < View.ChestItemOpenButtons.Length; i++)
                {
                    if (View.ChestItemOpenButtons[i] != null)
                    {
                        int index = i;
                        View.ChestItemOpenButtons[i].onClick.AddListener(() => OnChestItemButtonClicked(index));
                    }
                }
            }
            
            // Show chests tab by default
            View.ShowChestsTab();
        }

        private void OnChestsButtonClicked()
        {
            _hapticService?.ButtonPressed();
            View.ShowChestsTab();
        }

        private void OnBoostersButtonClicked()
        {
            _hapticService?.ButtonPressed();
            View.ShowBoostersTab();
        }

        private void OnCloseButtonClicked()
        {
            _hapticService?.ButtonPressed();
            
            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.ShopScreen);
        }

        private void OnChestItemButtonClicked(int index)
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Get the config for this chest item
            ShopChestItemConfig config = null;
            if (View.ChestItemConfigs != null && index < View.ChestItemConfigs.Length)
            {
                config = View.ChestItemConfigs[index];
            }

            if (config != null)
            {
                ShopChestItemUIPresenter.SetPendingConfig(config);
                _uiService?.Open(UIConstants.ShopChestItem);
            }
        }

        protected override void OnDispose()
        {
            if (View?.ChestsButton != null)
            {
                View.ChestsButton.onClick.RemoveListener(OnChestsButtonClicked);
            }

            if (View?.BoostersButton != null)
            {
                View.BoostersButton.onClick.RemoveListener(OnBoostersButtonClicked);
            }

            foreach (var closeButton in View.CloseButtons)
            {
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }

            // Unsubscribe from chest item buttons
            if (View?.ChestItemOpenButtons != null)
            {
                foreach (var button in View.ChestItemOpenButtons)
                {
                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
