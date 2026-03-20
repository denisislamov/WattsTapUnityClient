using System.Collections;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
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

            if (Model.Config != null && Model.Config.RewardAmount > 0)
            {
                ClaimReward(Model.Config.RewardAmount);
            }
            else
            {
                CloseAndReturn();
            }
        }

        private void ClaimReward(long rewardAmount)
        {
#if OLD_SERVER
            if (ServiceLocator.TryGet<IReferralAPIService>(out var apiService) && apiService.IsAuthenticated)
            {
                var runner = GetCoroutineRunner();
                if (runner != null)
                {
                    View.SetClaimButtonInteractable(false);
                    var request = new AddResourcesRequest
                    {
                        watts = (int)rewardAmount,
                        xp = 0
                    };
                    Debug.Log($"[ShopBoosterItemAnimation] Claiming reward via legacy API: {rewardAmount} watts");
                    runner.StartCoroutine(ClaimRewardCoroutine(apiService, request));
                    return;
                }
            }
#endif
            // Add reward locally (new server handles economy server-side via taps)
            Debug.Log($"[ShopBoosterItemAnimation] Adding reward locally: {rewardAmount} watts");
            AddRewardLocally(rewardAmount);
            CloseAndReturn();
        }

#if OLD_SERVER
        private IEnumerator ClaimRewardCoroutine(IReferralAPIService apiService, AddResourcesRequest request)
        {
            yield return apiService.AddResources(
                request,
                onSuccess: response =>
                {
                    Debug.Log($"<color=#00FF00>[ShopBoosterItemAnimation] Reward claimed: {response.message}</color>");

                    if (response.progress != null && ServiceLocator.TryGet<IPlayerService>(out var playerService))
                    {
                        playerService.LoadFromServer(
                            response.progress.level,
                            response.progress.watts,
                            response.progress.currentXp,
                            response.progress.totalXp
                        );
                    }

                    CloseAndReturn();
                },
                onError: error =>
                {
                    Debug.LogError($"[ShopBoosterItemAnimation] Failed to claim reward: {error}");
                    // Fallback: add locally on error
                    AddRewardLocally(request.watts);
                    CloseAndReturn();
                }
            );
        }
#endif

        private void AddRewardLocally(long amount)
        {
            if (ServiceLocator.TryGet<IPlayerService>(out var playerService))
            {
                playerService.AddWatts(amount);
                Debug.Log($"[ShopBoosterItemAnimation] Added {amount} watts locally");
            }
        }

        private void CloseAndReturn()
        {
            EnsureUIService();
            _uiService?.Close(UIConstants.ShopBoosterItemAnimation);
            _uiService?.Open(UIConstants.ShopScreen);
        }

        private MonoBehaviour GetCoroutineRunner()
        {
            if (View is MonoBehaviour viewMb)
                return viewMb;

            if (ServiceLocator.TryGet<IApplicationEntry>(out var appEntry) && appEntry is MonoBehaviour mb)
                return mb;

            return Object.FindObjectOfType<MonoBehaviour>();
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

