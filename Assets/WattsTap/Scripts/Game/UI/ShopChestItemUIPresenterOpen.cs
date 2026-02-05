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

        /// <summary>
        /// Set the config before opening the UI.
        /// Call this before opening ShopChestItemOpen screen.
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
            
            // Subscribe to animation reset event
            View.OnAnimationReset += OnAnimationReset;

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

            _uiService?.Close(UIConstants.ShopChestItemOpen);
        }

        private void OnAnimationReset()
        {
            _uiService?.Close(View);
            _uiService?.Open(UIConstants.ShopChestItemFinal);
        }

        protected override void OnDispose()
        {
            if (View != null)
            {
                View.OnAnimationReset -= OnAnimationReset;
            }
            
            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }
    }
}
