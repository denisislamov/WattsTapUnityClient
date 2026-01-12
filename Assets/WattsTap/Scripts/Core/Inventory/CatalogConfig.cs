using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// ScriptableObject that contains all available items in the game catalog.
    /// This is the data source for the CatalogService.
    /// In the future, this data can be loaded from a server.
    /// </summary>
    [CreateAssetMenu(menuName = "WattsTap/Inventory/Catalog Config")]
    public class CatalogConfig : BaseConfig
    {
        [Header("Items")]
        [Tooltip("All items available in the game catalog")]
        [SerializeField] private List<ItemData> _items = new List<ItemData>();
        
        /// <summary>
        /// All items in the catalog.
        /// </summary>
        public IReadOnlyList<ItemData> Items => _items;
        
        /// <summary>
        /// Get item by ID.
        /// </summary>
        public ItemData GetItem(string id)
        {
            foreach (var item in _items)
            {
                if (item != null && item.Id == id)
                {
                    return item;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Get all items of a specific type.
        /// </summary>
        public List<ItemData> GetItemsByType(ItemType type)
        {
            var result = new List<ItemData>();
            foreach (var item in _items)
            {
                if (item != null && item.ItemType == type)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        /// <summary>
        /// Get all items of a specific rarity.
        /// </summary>
        public List<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            var result = new List<ItemData>();
            foreach (var item in _items)
            {
                if (item != null && item.Rarity == rarity)
                {
                    result.Add(item);
                }
            }
            return result;
        }
        
        private void OnValidate()
        {
            ValidateUniqueIds();
        }
        
        private void ValidateUniqueIds()
        {
            var ids = new HashSet<string>();
            foreach (var item in _items)
            {
                if (item == null) continue;
                
                if (string.IsNullOrEmpty(item.Id))
                {
                    Debug.LogWarning($"[CatalogConfig] Item '{item.name}' has no ID!", item);
                    continue;
                }
                
                if (!ids.Add(item.Id))
                {
                    Debug.LogError($"[CatalogConfig] Duplicate item ID '{item.Id}' found!", item);
                }
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Add an item to the catalog (Editor only).
        /// </summary>
        public void AddItem(ItemData item)
        {
            if (item == null) return;
            if (!_items.Contains(item))
            {
                _items.Add(item);
            }
        }
        
        /// <summary>
        /// Remove an item from the catalog (Editor only).
        /// </summary>
        public void RemoveItem(ItemData item)
        {
            _items.Remove(item);
        }
#endif
    }
}
