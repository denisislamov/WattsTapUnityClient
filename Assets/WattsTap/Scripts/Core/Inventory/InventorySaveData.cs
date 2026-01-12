using System;
using System.Collections.Generic;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Serializable data structure for saving/loading inventory.
    /// Can be converted to JSON for server sync.
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        public List<InventoryItemSaveData> Items = new List<InventoryItemSaveData>();
        public int MaxCapacity;
    }
    
    /// <summary>
    /// Serializable data for a single inventory item.
    /// </summary>
    [Serializable]
    public class InventoryItemSaveData
    {
        public string InstanceId;
        public string ItemId;
        public long AcquiredAtTicks;
        public bool IsEquipped;
        
        public InventoryItemSaveData() { }
        
        public InventoryItemSaveData(InventoryItem item)
        {
            InstanceId = item.InstanceId;
            ItemId = item.Data?.Id;
            AcquiredAtTicks = item.AcquiredAt.Ticks;
            IsEquipped = item.IsEquipped;
        }
    }
}
