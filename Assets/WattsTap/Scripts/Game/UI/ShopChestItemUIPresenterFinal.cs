using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIPresenterFinal : UIBasePresenter<ShopChestItemUIViewFinal, ShopChestItemUIModelFinal>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static ShopChestItemConfig _pendingConfig;

        /// <summary>
        /// Set the config before opening the UI.
        /// Call this before opening ShopChestItemFinal screen.
        /// </summary>
        public static void SetPendingConfig(ShopChestItemConfig config)
        {
            _pendingConfig = config;
        }

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            if (View.CloseButton != null)
            {
                View.CloseButton.onClick.AddListener(OnCloseButtonClicked);
            }

            // Apply pending config if set
            if (_pendingConfig != null)
            {
                Model.SetConfig(_pendingConfig);
                View.UpdateFromConfig(_pendingConfig);
                _pendingConfig = null;
            }
        }

        private void OnCloseButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Закрываем текущий экран
            _uiService?.Close(UIConstants.ShopChestItemFinal);

            // Возвращаемся на экран ShopChestItemOpen без автозапуска анимации
            _uiService?.Open(UIConstants.ShopScreen);
        }

        protected override void OnDispose()
        {
            if (View?.CloseButton != null)
            {
                View.CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}
