using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIPresenter : UIBasePresenter<ShopChestItemUIView, ShopChestItemUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static ShopChestItemConfig _pendingConfig;

        /// <summary>
        /// Set the config before opening the UI.
        /// Call this before opening ShopChestItem screen.
        /// </summary>
        public static void SetPendingConfig(ShopChestItemConfig config)
        {
            _pendingConfig = config;
        }

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            if (View.BackButton != null)
            {
                View.BackButton.onClick.AddListener(OnBackButtonClicked);
            }

            if (View.OpenButton != null)
            {
                View.OpenButton.onClick.AddListener(OnOpenButtonClicked);
            }

            // Apply pending config if set
            if (_pendingConfig != null)
            {
                Model.SetConfig(_pendingConfig);
                View.UpdateFromConfig(_pendingConfig);
                _pendingConfig = null;
            }
        }

        private void OnBackButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.ShopChestItem);
        }

        private void OnOpenButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Передаём конфиг в новый экран
            ShopChestItemUIPresenterOpen.SetPendingConfig(Model.Config);

            // Открываем экран с анимацией открытия сундука
            _uiService?.Open(UIConstants.ShopChestItemOpen);

            // Закрываем текущий экран
            _uiService?.Close(UIConstants.ShopChestItem);
        }

        protected override void OnDispose()
        {
            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackButtonClicked);
            }

            if (View?.OpenButton != null)
            {
                View.OpenButton.onClick.RemoveListener(OnOpenButtonClicked);
            }
        }
    }
}
