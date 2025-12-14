using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    public class ShopBoosterItemUIPresenter : UIBasePresenter<ShopBoosterItemUIView, ShopBoosterItemUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static ShopBoosterItemConfig _pendingConfig;

        /// <summary>
        /// Set the config before opening the UI.
        /// Call this before opening ShopBoosterItem screen.
        /// </summary>
        public static void SetPendingConfig(ShopBoosterItemConfig config)
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

            _uiService?.Close(UIConstants.ShopBoosterItem);
        }

        protected override void OnDispose()
        {
            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackButtonClicked);
            }
        }
    }
}
