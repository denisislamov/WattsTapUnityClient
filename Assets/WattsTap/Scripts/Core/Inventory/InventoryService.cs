using System;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Service for managing player's inventory.
    /// Aggregates all items that the player owns (purchased, crafted, found, etc.)
    /// 
    /// Patterns used:
    /// - Observer Pattern: Events for item changes
    /// - Repository Pattern: Clean data access abstraction
    /// 
    /// Future extension: Can sync with server by implementing LoadFromServer/SaveToServer methods.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private const string StartConfigKey = "InventoryStartConfig";
        
        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private ICatalogService _catalogService;
        
        public int InitializationOrder => 25; // After CatalogService (20)
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            _catalogService = ServiceLocator.Get<ICatalogService>();
            
            // Try to load start config and apply it
            var configService = ServiceLocator.Get<IConfigService>();
            if (configService != null)
            {
                try
                {
                    var startConfig = configService.GetConfig<InventoryStartConfig>(StartConfigKey);
                    if (startConfig != null)
                    {
                        IsInitialized = true; // Set before applying to allow AddItem to work
                        startConfig.ApplyToInventory(this);
                    }
                }
                catch
                {
                    // Start config not registered, that's okay
                }
            }
            
            IsInitialized = true;
            Debug.Log("<color=#00AA00>[InventoryService] Initialized</color>");
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            _items.Clear();
            _catalogService = null;
            IsInitialized = false;
        }
        
        public IReadOnlyList<InventoryItem> Items => _items;
        public int ItemCount => _items.Count;
        public int MaxCapacity { get; set; }
        public bool IsFull => MaxCapacity > 0 && ItemCount >= MaxCapacity;
        
        public event Action<InventoryItem> OnItemAdded;
        public event Action<InventoryItem> OnItemRemoved;
        public event Action<InventoryItem, bool> OnItemEquipChanged;
        public event Action OnInventoryCleared;
        public event Action OnInventoryLoaded;
        
        public bool AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return false;
            }
            
            if (!_catalogService.TryGetItem(itemId, out var itemData))
            {
                Debug.LogWarning($"[InventoryService] Item not found in catalog: {itemId}");
                return false;
            }
            
            return AddItem(itemData);
        }
        
        public bool AddItem(ItemData itemData)
        {
            if (itemData == null)
            {
                return false;
            }
            
            if (IsFull)
            {
                Debug.LogWarning($"[InventoryService] Inventory full, could not add {itemData.Id}");
                return false;
            }
            
            var newItem = new InventoryItem(itemData);
            _items.Add(newItem);
            
            OnItemAdded?.Invoke(newItem);
            
            return true;
        }
        
        public bool RemoveItem(string instanceId)
        {
            var item = GetItemByInstanceId(instanceId);
            if (item == null) return false;
            
            _items.Remove(item);
            OnItemRemoved?.Invoke(item);
            
            return true;
        }
        
        public void ClearInventory()
        {
            _items.Clear();
            OnInventoryCleared?.Invoke();
        }
        
        public bool HasItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return false;
            
            foreach (var item in _items)
            {
                if (item.Data?.Id == itemId)
                {
                    return true;
                }
            }
            return false;
        }
        
        public IReadOnlyList<InventoryItem> GetItemsById(string itemId)
        {
            var result = new List<InventoryItem>();
            foreach (var item in _items)
            {
                if (item.Data?.Id == itemId)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        public IReadOnlyList<InventoryItem> GetItemsByType(ItemType type)
        {
            var result = new List<InventoryItem>();
            foreach (var item in _items)
            {
                if (item.Data?.ItemType == type)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        public InventoryItem GetItemByInstanceId(string instanceId)
        {
            foreach (var item in _items)
            {
                if (item.InstanceId == instanceId)
                {
                    return item;
                }
            }
            return null;
        }
        
        public IReadOnlyList<InventoryItem> GetEquippedItems()
        {
            var result = new List<InventoryItem>();
            foreach (var item in _items)
            {
                if (item.IsEquipped)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        public bool EquipItem(string instanceId)
        {
            var item = GetItemByInstanceId(instanceId);
            if (item == null || item.IsEquipped) return false;
            
            // Unequip any currently equipped item of the same type
            foreach (var equipped in _items)
            {
                if (equipped.IsEquipped && 
                    equipped.Data?.ItemType == item.Data?.ItemType &&
                    equipped.InstanceId != instanceId)
                {
                    equipped.IsEquipped = false;
                    OnItemEquipChanged?.Invoke(equipped, false);
                }
            }
            
            item.IsEquipped = true;
            OnItemEquipChanged?.Invoke(item, true);
            return true;
        }
        
        public bool UnequipItem(string instanceId)
        {
            var item = GetItemByInstanceId(instanceId);
            if (item == null || !item.IsEquipped) return false;
            
            item.IsEquipped = false;
            OnItemEquipChanged?.Invoke(item, false);
            return true;
        }
        
        public void UnequipAll()
        {
            foreach (var item in _items)
            {
                if (item.IsEquipped)
                {
                    item.IsEquipped = false;
                    OnItemEquipChanged?.Invoke(item, false);
                }
            }
        }
        
        public InventorySaveData GetSaveData()
        {
            var saveData = new InventorySaveData
            {
                MaxCapacity = MaxCapacity,
                Items = new List<InventoryItemSaveData>()
            };
            
            foreach (var item in _items)
            {
                saveData.Items.Add(new InventoryItemSaveData(item));
            }
            
            return saveData;
        }
        
        public void LoadFromSaveData(InventorySaveData saveData)
        {
            if (saveData == null) return;
            
            _items.Clear();
            MaxCapacity = saveData.MaxCapacity;
            
            foreach (var itemSave in saveData.Items)
            {
                if (!_catalogService.TryGetItem(itemSave.ItemId, out var itemData))
                {
                    Debug.LogWarning($"[InventoryService] Could not find item '{itemSave.ItemId}' in catalog during load");
                    continue;
                }
                
                var item = new InventoryItem(
                    itemSave.InstanceId,
                    itemData,
                    new DateTime(itemSave.AcquiredAtTicks),
                    itemSave.IsEquipped
                );
                
                _items.Add(item);
            }
            
            OnInventoryLoaded?.Invoke();
            Debug.Log($"<color=#00AA00>[InventoryService] Loaded {_items.Count} items from save data</color>");
        }
    }
}
