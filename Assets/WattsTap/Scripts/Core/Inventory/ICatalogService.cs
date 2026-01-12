using System;
using System.Collections.Generic;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Interface for the Catalog Service.
    /// Provides access to all available items in the game.
    /// </summary>
    public interface ICatalogService : IService
    {
        /// <summary>
        /// Get all available items in the catalog.
        /// </summary>
        IReadOnlyList<ItemData> GetAllItems();
        
        /// <summary>
        /// Get item by its unique ID.
        /// </summary>
        /// <param name="id">Unique item identifier</param>
        /// <returns>ItemData or null if not found</returns>
        ItemData GetItem(string id);
        
        /// <summary>
        /// Try to get item by its unique ID.
        /// </summary>
        /// <param name="id">Unique item identifier</param>
        /// <param name="item">Output item data</param>
        /// <returns>True if item was found</returns>
        bool TryGetItem(string id, out ItemData item);
        
        /// <summary>
        /// Get all items of a specific type.
        /// </summary>
        IReadOnlyList<ItemData> GetItemsByType(ItemType type);
        
        /// <summary>
        /// Get all items of a specific rarity.
        /// </summary>
        IReadOnlyList<ItemData> GetItemsByRarity(ItemRarity rarity);
        
        /// <summary>
        /// Get all items that match a filter.
        /// </summary>
        IReadOnlyList<ItemData> GetItems(Predicate<ItemData> filter);
        
        /// <summary>
        /// Check if an item exists in the catalog.
        /// </summary>
        bool HasItem(string id);
        
        /// <summary>
        /// Total number of items in the catalog.
        /// </summary>
        int ItemCount { get; }
        
        /// <summary>
        /// Fired when catalog is loaded or refreshed.
        /// </summary>
        event Action OnCatalogLoaded;
        
        /// <summary>
        /// Reload catalog data (for future server integration).
        /// </summary>
        void ReloadCatalog();
    }
}

