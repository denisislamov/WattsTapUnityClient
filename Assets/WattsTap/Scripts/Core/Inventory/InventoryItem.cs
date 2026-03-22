using System;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Represents an instance of an item in the player's inventory.
    /// Contains reference to item data and instance-specific information.
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        /// <summary>
        /// Unique instance ID for this inventory slot.
        /// </summary>
        public string InstanceId { get; private set; }
        
        /// <summary>
        /// Reference to the item data from catalog.
        /// </summary>
        public ItemData Data { get; private set; }
        
        /// <summary>
        /// Whether this item is currently equipped.
        /// </summary>
        public bool IsEquipped { get; set; }
        
        /// <summary>
        /// Current level of this item (1-based).
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// Timestamp when this item was acquired.
        /// </summary>
        public DateTime AcquiredAt { get; private set; }
        
        /// <summary>
        /// Creates a new inventory item instance.
        /// </summary>
        public InventoryItem(ItemData data)
        {
            InstanceId = Guid.NewGuid().ToString();
            Data = data;
            AcquiredAt = DateTime.UtcNow;
            IsEquipped = false;
            Level = 1;
        }
        
        /// <summary>
        /// Creates a new inventory item instance with specific instance ID (for loading from save).
        /// </summary>
        public InventoryItem(string instanceId, ItemData data, DateTime acquiredAt, bool isEquipped)
        {
            InstanceId = instanceId;
            Data = data;
            AcquiredAt = acquiredAt;
            IsEquipped = isEquipped;
            Level = 1;
        }
        
        /// <summary>
        /// Full constructor for server-sourced items.
        /// </summary>
        public InventoryItem(string instanceId, ItemData data, int level, bool isEquipped)
        {
            InstanceId = instanceId;
            Data = data;
            Level = level;
            IsEquipped = isEquipped;
            AcquiredAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Get the stat value at the current level.
        /// </summary>
        public float GetCurrentValue()
        {
            return Data?.GetValueAtLevel(Level) ?? 0f;
        }
        
        /// <summary>
        /// Get the stat value percent at the current level.
        /// </summary>
        public float GetCurrentValuePercent()
        {
            return Data?.GetValuePercentAtLevel(Level) ?? 0f;
        }
    }
}
