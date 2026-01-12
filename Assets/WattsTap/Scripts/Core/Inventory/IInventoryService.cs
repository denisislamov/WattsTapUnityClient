using System;
using System.Collections.Generic;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Interface for the Inventory Service.
    /// Manages all items that the player owns.
    /// </summary>
    public interface IInventoryService : IService
    {
        /// <summary>
        /// All items in the inventory.
        /// </summary>
        IReadOnlyList<InventoryItem> Items { get; }
        
        /// <summary>
        /// Current number of items.
        /// </summary>
        int ItemCount { get; }
        
        /// <summary>
        /// Maximum inventory capacity (0 = unlimited).
        /// </summary>
        int MaxCapacity { get; set; }
        
        /// <summary>
        /// Whether the inventory is at max capacity.
        /// </summary>
        bool IsFull { get; }
        
        /// <summary>
        /// Fired when an item is added to inventory.
        /// </summary>
        event Action<InventoryItem> OnItemAdded;
        
        /// <summary>
        /// Fired when an item is removed from inventory.
        /// </summary>
        event Action<InventoryItem> OnItemRemoved;
        
        /// <summary>
        /// Fired when an item is equipped or unequipped.
        /// </summary>
        event Action<InventoryItem, bool> OnItemEquipChanged;
        
        /// <summary>
        /// Fired when inventory is cleared.
        /// </summary>
        event Action OnInventoryCleared;
        
        /// <summary>
        /// Fired when inventory is loaded (from save or server).
        /// </summary>
        event Action OnInventoryLoaded;
        
        /// <summary>
        /// Add an item to the inventory by item ID.
        /// </summary>
        /// <param name="itemId">Item ID from catalog</param>
        /// <returns>True if item was added successfully</returns>
        bool AddItem(string itemId);
        
        /// <summary>
        /// Add an item to the inventory by ItemData.
        /// </summary>
        /// <param name="itemData">Item data from catalog</param>
        /// <returns>True if item was added successfully</returns>
        bool AddItem(ItemData itemData);
        
        /// <summary>
        /// Remove an item from the inventory by instance ID.
        /// </summary>
        /// <param name="instanceId">Instance ID of the inventory item</param>
        /// <returns>True if item was removed successfully</returns>
        bool RemoveItem(string instanceId);
        
        /// <summary>
        /// Clear all items from inventory.
        /// </summary>
        void ClearInventory();
        
        /// <summary>
        /// Check if player has an item with specific item ID.
        /// </summary>
        /// <param name="itemId">Item ID to check</param>
        /// <returns>True if player has the item</returns>
        bool HasItem(string itemId);
        
        /// <summary>
        /// Get all inventory items with a specific item ID.
        /// </summary>
        /// <param name="itemId">Item ID to find</param>
        /// <returns>List of matching inventory items</returns>
        IReadOnlyList<InventoryItem> GetItemsById(string itemId);
        
        /// <summary>
        /// Get all inventory items of a specific type.
        /// </summary>
        /// <param name="type">Item type to find</param>
        /// <returns>List of matching inventory items</returns>
        IReadOnlyList<InventoryItem> GetItemsByType(ItemType type);
        
        /// <summary>
        /// Get inventory item by instance ID.
        /// </summary>
        /// <param name="instanceId">Instance ID to find</param>
        /// <returns>Inventory item or null</returns>
        InventoryItem GetItemByInstanceId(string instanceId);
        
        /// <summary>
        /// Get all equipped items.
        /// </summary>
        /// <returns>List of equipped items</returns>
        IReadOnlyList<InventoryItem> GetEquippedItems();
        
        /// <summary>
        /// Equip an item.
        /// </summary>
        /// <param name="instanceId">Instance ID of item to equip</param>
        /// <returns>True if item was equipped</returns>
        bool EquipItem(string instanceId);
        
        /// <summary>
        /// Unequip an item.
        /// </summary>
        /// <param name="instanceId">Instance ID of item to unequip</param>
        /// <returns>True if item was unequipped</returns>
        bool UnequipItem(string instanceId);
        
        /// <summary>
        /// Unequip all items.
        /// </summary>
        void UnequipAll();
        
        /// <summary>
        /// Get serializable data for saving.
        /// </summary>
        InventorySaveData GetSaveData();
        
        /// <summary>
        /// Load inventory from save data.
        /// </summary>
        void LoadFromSaveData(InventorySaveData saveData);
    }
}
