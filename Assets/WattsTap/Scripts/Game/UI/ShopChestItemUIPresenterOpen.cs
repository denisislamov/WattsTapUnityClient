using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopChestItemUIPresenterOpen : UIBasePresenter<ShopChestItemUIViewOpen, ShopChestItemUIModelOpen>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static ShopChestItemConfig _pendingConfig;
        private static bool _autoPlayAnimation = true;

        /// <summary>
        /// Set the config before opening the UI.
        /// Call this before opening ShopChestItemOpen screen.
        /// </summary>
        public static void SetPendingConfig(ShopChestItemConfig config, bool autoPlayAnimation = true)
        {
            _pendingConfig = config;
            _autoPlayAnimation = autoPlayAnimation;
        }

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            if (View.BackButton != null)
            {
                View.BackButton.onClick.AddListener(OnBackButtonClicked);
            }

            // Подписываемся на событие завершения анимации
            View.OnOpenAnimationComplete += OnOpenAnimationComplete;

            // Apply pending config if set
            if (_pendingConfig != null)
            {
                Model.SetConfig(_pendingConfig);
                View.UpdateFromConfig(_pendingConfig);
                _pendingConfig = null;
            }
            
            // Запускаем анимацию открытия сундука только если установлен флаг
            if (_autoPlayAnimation)
            {
                View.PlayOpenAnimation();
            }
            
            // Сбрасываем флаг для следующего использования
            _autoPlayAnimation = true;
        }

        private void OnOpenAnimationComplete()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Передаём конфиг в финальный экран
            ShopChestItemUIPresenterFinal.SetPendingConfig(Model.Config);

            // Открываем финальный экран
            _uiService?.Open(UIConstants.ShopChestItemFinal);

            // Закрываем текущий экран
            _uiService?.Close(UIConstants.ShopChestItemOpen);
        }

        private void OnBackButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.ShopChestItemOpen);
        }

        protected override void OnDispose()
        {
            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackButtonClicked);
            }

            if (View != null)
            {
                View.OnOpenAnimationComplete -= OnOpenAnimationComplete;
            }
        }
    }
}
