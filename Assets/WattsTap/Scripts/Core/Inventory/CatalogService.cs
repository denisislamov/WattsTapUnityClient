using System;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Service that provides access to the game's item catalog.
    /// Contains all items that can be purchased, crafted, or obtained in the game.
    /// 
    /// Pattern: Repository Pattern - provides a clean abstraction layer for item data access.
    /// Future extension: Can be modified to load data from a server by implementing
    /// an async LoadFromServer() method.
    /// </summary>
    public class CatalogService : ICatalogService
    {
        private const string CatalogConfigKey = "CatalogConfig";
        
        private CatalogConfig _config;
        private Dictionary<string, ItemData> _itemsById;
        private Dictionary<ItemType, List<ItemData>> _itemsByType;
        private Dictionary<ItemRarity, List<ItemData>> _itemsByRarity;
        
        public int InitializationOrder => 20;
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            var configService = ServiceLocator.Get<IConfigService>();
            _config = configService.GetConfig<CatalogConfig>(CatalogConfigKey);
            
            BuildIndexes();
            
            IsInitialized = true;
            OnCatalogLoaded?.Invoke();
            
            Debug.Log($"<color=#00AA00>[CatalogService] Initialized with {ItemCount} items</color>");
        }
        
        public void Shutdown()
        {
            if (!IsInitialized) return;
            
            _itemsById?.Clear();
            _itemsByType?.Clear();
            _itemsByRarity?.Clear();
            _config = null;
            IsInitialized = false;
        }
        
        public event Action OnCatalogLoaded;
        
        public int ItemCount => _config?.Items?.Count ?? 0;
        
        public IReadOnlyList<ItemData> GetAllItems()
        {
            return _config?.Items ?? new List<ItemData>();
        }
        
        public ItemData GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            
            if (_itemsById != null && _itemsById.TryGetValue(id, out var item))
            {
                return item;
            }
            
            return null;
        }
        
        public bool TryGetItem(string id, out ItemData item)
        {
            item = GetItem(id);
            return item != null;
        }
        
        public IReadOnlyList<ItemData> GetItemsByType(ItemType type)
        {
            if (_itemsByType != null && _itemsByType.TryGetValue(type, out var items))
            {
                return items;
            }
            
            return new List<ItemData>();
        }
        
        public IReadOnlyList<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            if (_itemsByRarity != null && _itemsByRarity.TryGetValue(rarity, out var items))
            {
                return items;
            }
            
            return new List<ItemData>();
        }
        
        public IReadOnlyList<ItemData> GetItems(Predicate<ItemData> filter)
        {
            if (filter == null || _config?.Items == null)
            {
                return new List<ItemData>();
            }
            
            var result = new List<ItemData>();
            foreach (var item in _config.Items)
            {
                if (item != null && filter(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        public bool HasItem(string id)
        {
            return !string.IsNullOrEmpty(id) && _itemsById != null && _itemsById.ContainsKey(id);
        }
        
        public void ReloadCatalog()
        {
            BuildIndexes();
            OnCatalogLoaded?.Invoke();
        }
        
        private void BuildIndexes()
        {
            _itemsById = new Dictionary<string, ItemData>();
            _itemsByType = new Dictionary<ItemType, List<ItemData>>();
            _itemsByRarity = new Dictionary<ItemRarity, List<ItemData>>();
            
            if (_config?.Items == null) return;
            
            foreach (var item in _config.Items)
            {
                if (item == null) continue;
                
                // Index by ID
                if (!string.IsNullOrEmpty(item.Id))
                {
                    if (_itemsById.ContainsKey(item.Id))
                    {
                        Debug.LogWarning($"[CatalogService] Duplicate item ID: {item.Id}");
                    }
                    else
                    {
                        _itemsById[item.Id] = item;
                    }
                }
                
                // Index by Type
                if (!_itemsByType.ContainsKey(item.ItemType))
                {
                    _itemsByType[item.ItemType] = new List<ItemData>();
                }
                _itemsByType[item.ItemType].Add(item);
                
                // Index by Rarity
                if (!_itemsByRarity.ContainsKey(item.Rarity))
                {
                    _itemsByRarity[item.Rarity] = new List<ItemData>();
                }
                _itemsByRarity[item.Rarity].Add(item);
            }
        }
        
        // TODO: Add these methods when implementing server integration
        // public IEnumerator LoadFromServer(Action<bool> onComplete)
        // {
        //     // Make API call to get catalog data
        //     // Parse response and populate items
        //     // Call BuildIndexes()
        //     yield return null;
        // }
    }
}
