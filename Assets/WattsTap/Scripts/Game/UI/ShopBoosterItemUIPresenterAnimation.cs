using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Shop;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Presenter for the booster purchase animation screen.
    /// Receives the booster config via static pending data (same pattern as ShopChestItemUIPresenterOpen).
    ///
    /// Flow:
    /// 1. Screen opens → animation plays automatically (icon reveal, counter, rotation).
    /// 2. Animation completes → Claim button becomes interactable.
    /// 3. User presses Claim → screen closes, returns to ShopScreen.
    /// </summary>
    public class ShopBoosterItemUIPresenterAnimation : UIBasePresenter<ShopBoosterItemUIViewAnimation, ShopBoosterItemUIModelAnimation>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static ShopBoosterItemConfig _pendingConfig;

        /// <summary>
        /// Set the config before opening the animation screen.
        /// Call this before opening ShopBoosterItemAnimation.
        /// </summary>
        public static void SetPendingConfig(ShopBoosterItemConfig config)
        {
            _pendingConfig = config;
        }

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            View.OnBackClicked += OnBackClicked;
            View.OnClaimClicked += OnClaimClicked;

            View.SetClaimButtonInteractable(false);

            if (_pendingConfig != null)
            {
                Model.SetConfig(_pendingConfig);
                View.UpdateFromConfig(_pendingConfig);
                _pendingConfig = null;

                Debug.Log($"[ShopBoosterItemAnimation] Starting animation for booster: {Model.Config.ItemName}");
                View.PlayAnimation();
            }
            else
            {
                Debug.LogWarning("[ShopBoosterItemAnimation] No pending config set.");
            }
        }

        private void OnBackClicked()
        {
            _hapticService?.ButtonPressed();
            EnsureUIService();

            _uiService?.Close(UIConstants.ShopBoosterItemAnimation);
            _uiService?.Open(UIConstants.ShopScreen);
        }

        private void OnClaimClicked()
        {
            _hapticService?.ButtonPressed();

            if (!View.IsAnimationFinished) return;

            EnsureUIService();

            _uiService?.Close(UIConstants.ShopBoosterItemAnimation);
            _uiService?.Open(UIConstants.ShopScreen);
        }

        private void EnsureUIService()
        {
            if (_uiService == null)
                ServiceLocator.TryGet(out _uiService);
        }

        protected override void OnDispose()
        {
            if (View != null)
            {
                View.OnBackClicked -= OnBackClicked;
                View.OnClaimClicked -= OnClaimClicked;
            }
        }
    }
}

