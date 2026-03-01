using System.Collections.Generic;
using WattsTap.Core.Inventory;
using WattsTap.Core.UI;

namespace WattsTap.Game.UI
{
    /// <summary>
    /// Model for the Merge Animation screen.
    /// Contains data about items that were placed in merge slots.
    /// </summary>
    public class MergeScreenUIModelAnimation : UIBaseModel
    {
        private List<InventoryItem> _mergeSlotItems;

        /// <summary>
        /// Items placed in merge slots (up to 3).
        /// </summary>
        public IReadOnlyList<InventoryItem> MergeSlotItems => _mergeSlotItems;

        public override void Initialize()
        {
            base.Initialize();
            _mergeSlotItems = new List<InventoryItem>();
        }

        /// <summary>
        /// Set the items that were selected for merge.
        /// </summary>
        public void SetMergeItems(IReadOnlyList<InventoryItem> items)
        {
            _mergeSlotItems = new List<InventoryItem>(items);
        }

        /// <summary>
        /// Get item at specific slot index.
        /// </summary>
        public InventoryItem GetSlotItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _mergeSlotItems.Count)
                return null;

            return _mergeSlotItems[slotIndex];
        }

        /// <summary>
        /// Number of items in merge slots.
        /// </summary>
        public int SlotCount => _mergeSlotItems?.Count ?? 0;

        public override void Dispose()
        {
            _mergeSlotItems?.Clear();
            _mergeSlotItems = null;
            base.Dispose();
        }
    }
}

