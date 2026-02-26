using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WattsTap.Core;
using WattsTap.Core.Inventory;
using WattsTap.Core.UI;
using WattsTap.Game.UI.Components.Inventory;

namespace WattsTap.Game.UI
{
    public class MergeScreenUIView : UIBaseView<MergeScreenUIPresenter>
    {
        [Header("Skinning")]
        [SerializeField] private MainMenuThemeManager.SkinTokenBinding[] _skinBindings;
        
        private MainMenuThemeManager _themeManager;
        
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _inventoryButton;

        [Header("Inventory Grid")]
        [SerializeField] private Transform _inventoryContainer;
        [SerializeField] private MergeItemElementView _itemPrefab;

        [Header("Merge Slots")]
        [SerializeField] private MergeSlotView _mergeSlot1;
        [SerializeField] private MergeSlotView _mergeSlot2;
        [SerializeField] private MergeSlotView _mergeSlot3;

        [Header("Merge Action")]
        [SerializeField] private Button _mergeButton;

        private const int MaxMergeSlots = 3;

        private readonly List<MergeItemElementView> _itemViews = new List<MergeItemElementView>();
        private readonly List<MergeItemElementView> _selectedItems = new List<MergeItemElementView>();

        public event Action<MergeItemElementView> OnItemClicked;
        public event Action<MergeItemElementView, bool> OnItemSelectionChanged;
        public event Action OnMergeButtonClicked;

        public Button BackButton => _backButton;
        public Button InventoryButton => _inventoryButton;
        public Button MergeButton => _mergeButton;
        public IReadOnlyList<MergeItemElementView> SelectedItems => _selectedItems;

        private MergeSlotView[] MergeSlots => new[] { _mergeSlot1, _mergeSlot2, _mergeSlot3 };

        /// <summary>
        /// Populate the inventory grid with items.
        /// </summary>
        public void PopulateInventory(IReadOnlyList<InventoryItem> items)
        {
            ClearInventoryViews();
            ClearAllSlots();
            
            if (_itemPrefab == null || _inventoryContainer == null)
            {
                Debug.LogWarning("[MergeScreenUIView] Item prefab or container not set");
                return;
            }
            
            foreach (var item in items)
            {
                var itemView = Instantiate(_itemPrefab, _inventoryContainer);
                itemView.Setup(item);
                itemView.OnSingleClick += HandleItemClick;
                itemView.OnSelectionChanged += HandleItemSelectionChanged;
                _itemViews.Add(itemView);
            }

            UpdateMergeButtonState();
        }

        private void HandleItemClick(MergeItemElementView itemView)
        {
            OnItemClicked?.Invoke(itemView);
        }

        private void HandleItemSelectionChanged(MergeItemElementView itemView, bool isSelected)
        {
            if (isSelected)
            {
                if (!TrySelectItem(itemView))
                {
                    // Selection rejected — revert the visual state silently
                    itemView.SetSelected(false, notify: false);
                    return;
                }
            }
            else
            {
                DeselectItem(itemView);
            }

            OnItemSelectionChanged?.Invoke(itemView, itemView.IsSelected);
        }

        /// <summary>
        /// Attempts to add item to a merge slot.
        /// Returns false if validation fails (max reached, type/rarity mismatch).
        /// </summary>
        private bool TrySelectItem(MergeItemElementView itemView)
        {
            // Already at max
            if (_selectedItems.Count >= MaxMergeSlots)
                return false;

            // Validate type & rarity match with already selected items
            if (_selectedItems.Count > 0)
            {
                var firstData = _selectedItems[0].InventoryItem.Data;
                var candidateData = itemView.InventoryItem.Data;

                if (candidateData.ItemType != firstData.ItemType ||
                    candidateData.Rarity != firstData.Rarity)
                {
                    Debug.Log("[MergeScreenUIView] Item type or rarity does not match the current selection.");
                    return false;
                }
            }

            _selectedItems.Add(itemView);
            RefreshSlots();
            UpdateMergeButtonState();
            FilterInventoryGrid();
            return true;
        }

        /// <summary>
        /// Removes item from merge selection.
        /// </summary>
        private void DeselectItem(MergeItemElementView itemView)
        {
            _selectedItems.Remove(itemView);
            RefreshSlots();
            UpdateMergeButtonState();
            FilterInventoryGrid();
        }

        /// <summary>
        /// Called by MergeSlotView click — deselects the item in that slot.
        /// </summary>
        private void HandleSlotClicked(MergeSlotView slotView)
        {
            if (slotView.Item == null) return;

            // Find the matching MergeItemElementView
            var match = _selectedItems.FirstOrDefault(v => v.InventoryItem == slotView.Item);
            if (match != null)
            {
                match.SetSelected(false); // Will trigger HandleItemSelectionChanged → DeselectItem
            }
        }

        private void RefreshSlots()
        {
            var slots = MergeSlots;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;

                if (i < _selectedItems.Count)
                    slots[i].SetItem(_selectedItems[i].InventoryItem);
                else
                    slots[i].ClearSlot();
            }
        }

        private void ClearAllSlots()
        {
            // Deselect all items in grid
            foreach (var item in _selectedItems)
            {
                if (item != null)
                    item.SetSelected(false, notify: false);
            }
            _selectedItems.Clear();

            foreach (var slot in MergeSlots)
            {
                if (slot != null)
                    slot.ClearSlot();
            }

            UpdateMergeButtonState();
            FilterInventoryGrid();
        }

        /// <summary>
        /// Filters the inventory grid based on the current selection.
        /// When at least one item is selected, only items with matching ItemType and Rarity are visible.
        /// When no items are selected, all items are visible.
        /// </summary>
        private void FilterInventoryGrid()
        {
            if (_selectedItems.Count == 0)
            {
                // No selection — show all items
                foreach (var view in _itemViews)
                {
                    if (view != null)
                        view.gameObject.SetActive(true);
                }
                return;
            }

            var firstData = _selectedItems[0].InventoryItem.Data;
            var requiredType = firstData.ItemType;
            var requiredRarity = firstData.Rarity;

            foreach (var view in _itemViews)
            {
                if (view == null) continue;

                var data = view.InventoryItem?.Data;
                if (data == null)
                {
                    view.gameObject.SetActive(false);
                    continue;
                }

                bool matches = data.ItemType == requiredType && data.Rarity == requiredRarity;
                view.gameObject.SetActive(matches);
            }
        }

        private void UpdateMergeButtonState()
        {
            if (_mergeButton != null)
            {
                _mergeButton.interactable = _selectedItems.Count >= MaxMergeSlots;
            }
        }

        private void SubscribeSlots()
        {
            foreach (var slot in MergeSlots)
            {
                if (slot != null)
                    slot.OnSlotClicked += HandleSlotClicked;
            }
        }

        private void UnsubscribeSlots()
        {
            foreach (var slot in MergeSlots)
            {
                if (slot != null)
                    slot.OnSlotClicked -= HandleSlotClicked;
            }
        }

        private void ClearInventoryViews()
        {
            foreach (var view in _itemViews)
            {
                if (view != null)
                {
                    view.OnSingleClick -= HandleItemClick;
                    view.OnSelectionChanged -= HandleItemSelectionChanged;
                    Destroy(view.gameObject);
                }
            }
            _itemViews.Clear();
        }

        #region Skinning

        private void OnEnable()
        {
            SubscribeSlots();

            if (_mergeButton != null)
            {
                _mergeButton.interactable = false;
                _mergeButton.onClick.AddListener(HandleMergeButtonClick);
            }

            _themeManager = ServiceLocator.Get<MainMenuThemeManager>();
            
            if (_themeManager != null)
            {
                _themeManager.SkinChanged += OnSkinChanged;

                if (_themeManager.CurrentSkin != null)
                {
                    _themeManager.ApplySkin(_themeManager.CurrentSkin, _skinBindings, this);
                    return;
                }
                
                _themeManager.ApplySkin(null, _skinBindings, this);
            }
        }

        private void OnDisable()
        {
            UnsubscribeSlots();

            if (_mergeButton != null)
            {
                _mergeButton.onClick.RemoveListener(HandleMergeButtonClick);
            }

            if (_themeManager != null)
            {
                _themeManager.SkinChanged -= OnSkinChanged;
            }
        }

        private void HandleMergeButtonClick()
        {
            OnMergeButtonClicked?.Invoke();
        }
        
        private void OnSkinChanged(MainMenuSkinDefinition skin)
        {
            if (_themeManager != null)
            {
                _themeManager.ApplySkin(skin, _skinBindings, this);
            }
        }

        #endregion

        protected override void OnDestroy()
        {
            ClearInventoryViews();
            base.OnDestroy();
        }
    }
}

