using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Presenter for the Merge Animation screen.
    /// Receives merge slot items via static pending data (same pattern as ShopChestItemUIPresenterOpen).
    ///
    /// Flow:
    /// 1. Screen opens → merge animation starts automatically.
    /// 2. Animation completes → tap zone becomes active.
    /// 3. User taps the tap zone → screen closes, returns to MergeScreen.
    /// </summary>
    public class MergeScreenUIPresenterAnimation : UIBasePresenter<MergeScreenUIViewAnimation, MergeScreenUIModelAnimation>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static List<InventoryItem> _pendingMergeItems;
        private static MergeResultInfo _pendingMergeResult;

        /// <summary>
        /// Set the items selected for merge before opening the animation screen.
        /// </summary>
        public static void SetPendingMergeItems(IReadOnlyList<InventoryItem> items)
        {
            _pendingMergeItems = items?.ToList();
        }

        /// <summary>
        /// Set the computed merge result info before opening the animation screen.
        /// </summary>
        public static void SetPendingMergeResult(MergeResultInfo result)
        {
            _pendingMergeResult = result;
        }

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            View.OnBackClicked += OnBackClicked;
            View.OnAnimationComplete += OnAnimationComplete;
            View.OnTapZoneClicked += OnTapZoneClicked;

            View.SetTapZoneActive(false);
            View.SetConfirmButtonInteractable(false);

            if (_pendingMergeItems != null && _pendingMergeItems.Count > 0)
            {
                Model.SetMergeItems(_pendingMergeItems);
                View.DisplayMergeItems(Model.MergeSlotItems);

                if (_pendingMergeResult != null)
                {
                    Model.SetMergeResult(_pendingMergeResult);
                    View.DisplayMergeResult(_pendingMergeResult);
                }

                _pendingMergeItems = null;
                _pendingMergeResult = null;

                var items = Model.MergeSlotItems;
                if (items != null && items.Count >= 3)
                {
                    Debug.Log($"[MergeScreenUIPresenterAnimation] Auto-starting merge animation: {string.Join(", ", items.Select(i => i.Data.DisplayName))}");
                }

                View.SetStatusText("Merging...");
                View.PlayMergeAnimation();
            }
            else
            {
                Debug.LogWarning("[MergeScreenUIPresenterAnimation] No pending merge items set.");
            }
        }

        private void OnBackClicked()
        {
            _hapticService?.ButtonPressed();
            EnsureUIService();

            _uiService?.Close(UIConstants.MergeScreenAnimation);
            _uiService?.Open(UIConstants.MergeScreen);
        }

        private void OnTapZoneClicked()
        {
            _hapticService?.ButtonPressed();

            if (View.IsAnimationFinished)
            {
                EnsureUIService();
                _uiService?.Close(UIConstants.MergeScreenAnimation);
                _uiService?.Open(UIConstants.MergeScreen);
            }
        }

        private void OnAnimationComplete()
        {
            Debug.Log("[MergeScreenUIPresenterAnimation] Animation complete. Tap to close.");

            View.SetStatusText("Tap to continue");
            View.SetTapZoneActive(true);
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
                View.OnAnimationComplete -= OnAnimationComplete;
                View.OnTapZoneClicked -= OnTapZoneClicked;
            }
        }
    }
}
