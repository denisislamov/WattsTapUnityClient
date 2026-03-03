using System.Linq;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    public class MergeScreenUIPresenter : UIBasePresenter<MergeScreenUIView, MergeScreenUIModel>
    {
        private IUIService _uiService;
        private IHapticFeedbackService _hapticService;

        protected override void OnInit()
        {
            ServiceLocator.TryGet(out _uiService);
            ServiceLocator.TryGet(out _hapticService);

            // Populate inventory
            PopulateInventory();

            // Subscribe to view events
            View.OnItemClicked += OnItemClicked;
            View.OnItemSelectionChanged += OnItemSelectionChanged;
            View.OnMergeButtonClicked += OnMergeButtonClicked;

            if (View.BackButton != null)
            {
                View.BackButton.onClick.AddListener(OnBackButtonClicked);
            }

            if (View.InventoryButton != null)
            {
                View.InventoryButton.onClick.AddListener(OnInventoryButtonClicked);
            }
        }

        private void PopulateInventory()
        {
            var items = Model.GetAllItems();
            View.PopulateInventory(items);
        }

        private void OnItemClicked(MergeItemElementView itemView)
        {
            _hapticService?.ButtonPressed();
        }

        private void OnItemSelectionChanged(MergeItemElementView itemView, bool isSelected)
        {
            _hapticService?.ButtonPressed();
        }

        private void OnMergeButtonClicked()
        {
            _hapticService?.ButtonPressed();

            var selected = View.SelectedItems;
            if (selected == null || selected.Count < 3) return;

            var items = selected.Select(v => v.InventoryItem).ToList();
            Debug.Log($"[MergeScreenUIPresenter] Merge requested: {string.Join(", ", items.Select(i => i.Data.DisplayName))}");

            // Pass selected items and merge result to the animation screen
            MergeScreenUIPresenterAnimation.SetPendingMergeItems(items);
            MergeScreenUIPresenterAnimation.SetPendingMergeResult(View.CurrentMergeResult);

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            // Open merge animation screen and close current screen
            _uiService?.Open(UIConstants.MergeScreenAnimation);
            _uiService?.Close(UIConstants.MergeScreen);
        }

        private void OnBackButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.MergeScreen);
            _uiService?.Open(UIConstants.InventoryScreen);
        }

        private void OnInventoryButtonClicked()
        {
            _hapticService?.ButtonPressed();

            if (_uiService == null)
            {
                ServiceLocator.TryGet(out _uiService);
            }

            _uiService?.Close(UIConstants.MergeScreen);
            _uiService?.Open(UIConstants.InventoryScreen);
        }

        protected override void OnDispose()
        {
            if (View != null)
            {
                View.OnItemClicked -= OnItemClicked;
                View.OnItemSelectionChanged -= OnItemSelectionChanged;
                View.OnMergeButtonClicked -= OnMergeButtonClicked;
            }

            if (View?.BackButton != null)
            {
                View.BackButton.onClick.RemoveListener(OnBackButtonClicked);
            }

            if (View?.InventoryButton != null)
            {
                View.InventoryButton.onClick.RemoveListener(OnInventoryButtonClicked);
            }
        }
    }
}
