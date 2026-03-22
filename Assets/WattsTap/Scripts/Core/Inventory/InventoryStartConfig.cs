using System;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Configuration for initial inventory items.
    /// Use this to define items that should be added to the player's inventory at game start or for testing.
    /// </summary>
    [CreateAssetMenu(menuName = "WattsTap/Inventory/Inventory Start Config")]
    public class InventoryStartConfig : BaseConfig
    {
        [Tooltip("If true — load inventory from this local config (debug/testing). If false — load from server /game/inventory")]
        [SerializeField] private bool _isDebug;
        
        [Tooltip("Items to add to inventory on initialization")]
        [SerializeField] private List<InventoryStartItem> _startItems = new List<InventoryStartItem>();
        
        /// <summary>
        /// All items to be added to inventory.
        /// </summary>
        public IReadOnlyList<InventoryStartItem> StartItems => _startItems;
        
        /// <summary>
        /// If true — use local inventory from this config (debug/testing mode).
        /// If false — load inventory from server /game/inventory.
        /// </summary>
        public bool IsDebug => _isDebug;
        
        /// <summary>
        /// Apply this configuration to the inventory service.
        /// </summary>
        public void ApplyToInventory(IInventoryService inventoryService)
        {
            if (inventoryService == null)
            {
                Debug.LogError("[InventoryStartConfig] InventoryService is null");
                return;
            }
            
            foreach (var startItem in _startItems)
            {
                if (startItem.ItemData == null)
                {
                    Debug.LogWarning("[InventoryStartConfig] Skipping null item data");
                    continue;
                }
                
                for (int i = 0; i < startItem.Count; i++)
                {
                    bool added = inventoryService.AddItem(startItem.ItemData);
                    if (!added)
                    {
                        Debug.LogWarning($"[InventoryStartConfig] Failed to add item: {startItem.ItemData.Id}");
                        break;
                    }
                }
                
                // Auto-equip if specified
                if (startItem.AutoEquip && startItem.Count > 0)
                {
                    var items = inventoryService.GetItemsById(startItem.ItemData.Id);
                    if (items.Count > 0)
                    {
                        inventoryService.EquipItem(items[0].InstanceId);
                    }
                }
            }
            
            Debug.Log($"<color=#00AA00>[InventoryStartConfig] Applied {_startItems.Count} item types to inventory</color>");
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Add an item to the start config (Editor only).
        /// </summary>
        public void AddStartItem(ItemData itemData, int count = 1, bool autoEquip = false)
        {
            if (itemData == null) return;
            
            // Check if item already exists
            foreach (var existingItem in _startItems)
            {
                if (existingItem.ItemData == itemData)
                {
                    existingItem.Count += count;
                    return;
                }
            }
            
            _startItems.Add(new InventoryStartItem
            {
                ItemData = itemData,
                Count = count,
                AutoEquip = autoEquip
            });
        }
        
        /// <summary>
        /// Remove an item from the start config (Editor only).
        /// </summary>
        public void RemoveStartItem(int index)
        {
            if (index >= 0 && index < _startItems.Count)
            {
                _startItems.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// Clear all items (Editor only).
        /// </summary>
        public void ClearAllItems()
        {
            _startItems.Clear();
        }
        
        /// <summary>
        /// Get mutable list for editor (Editor only).
        /// </summary>
        public List<InventoryStartItem> GetEditableItems()
        {
            return _startItems;
        }
#endif
    }
    
    /// <summary>
    /// Represents an item entry in the start inventory configuration.
    /// </summary>
    [Serializable]
    public class InventoryStartItem
    {
        [Tooltip("Item to add")]
        public ItemData ItemData;
        
        [Tooltip("Number of instances to add")]
        [Min(1)]
        public int Count = 1;
        
        [Tooltip("Automatically equip this item (first instance only)")]
        public bool AutoEquip;
    }
}

