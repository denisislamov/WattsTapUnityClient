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
    /// </summary>
    public class MergeScreenUIPresenterAnimation : UIBasePresenter<MergeScreenUIViewAnimation, MergeScreenUIModelAnimation>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        private static List<InventoryItem> _pendingMergeItems;
        private static MergeResultInfo _pendingMergeResult;

        /// <summary>
        /// Set the items selected for merge before opening the animation screen.
        /// Call this before opening MergeScreenAnimation via UIService.
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

            // Subscribe to view events
            View.OnBackClicked += OnBackClicked;
            View.OnConfirmMergeClicked += OnConfirmMergeClicked;
            View.OnAnimationComplete += OnAnimationComplete;

            // Apply pending merge items
            if (_pendingMergeItems != null && _pendingMergeItems.Count > 0)
            {
                Model.SetMergeItems(_pendingMergeItems);
                View.DisplayMergeItems(Model.MergeSlotItems);

                // Apply merge result info
                if (_pendingMergeResult != null)
                {
                    Model.SetMergeResult(_pendingMergeResult);
                    View.DisplayMergeResult(_pendingMergeResult);
                }

                _pendingMergeItems = null;
                _pendingMergeResult = null;
            }
            else
            {
                Debug.LogWarning("[MergeScreenUIPresenterAnimation] No pending merge items set.");
            }
        }

        private void OnBackClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
                ServiceLocator.TryGet(out _uiService);

            _uiService?.Close(UIConstants.MergeScreenAnimation);
            _uiService?.Open(UIConstants.MergeScreen);
        }

        private void OnConfirmMergeClicked()
        {
            _hapticService?.ButtonPressed();

            var items = Model.MergeSlotItems;
            if (items == null || items.Count < 3)
            {
                Debug.LogWarning("[MergeScreenUIPresenterAnimation] Not enough items to merge.");
                return;
            }

            View.SetConfirmButtonInteractable(false);
            View.SetStatusText("Merging...");

            Debug.Log($"[MergeScreenUIPresenterAnimation] Confirm merge: {string.Join(", ", items.Select(i => i.Data.DisplayName))}");

            // TODO: Call merge service logic, play animation, then call View.NotifyAnimationComplete()
        }

        private void OnAnimationComplete()
        {
            Debug.Log("[MergeScreenUIPresenterAnimation] Animation complete.");

            // TODO: Show result item, or navigate to result screen
        }

        protected override void OnDispose()
        {
            if (View != null)
            {
                View.OnBackClicked -= OnBackClicked;
                View.OnConfirmMergeClicked -= OnConfirmMergeClicked;
                View.OnAnimationComplete -= OnAnimationComplete;
            }
        }
    }
}

