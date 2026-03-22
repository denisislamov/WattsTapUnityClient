using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.API;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Service that provides access to the game's item catalog.
    /// Supports both local (ScriptableObject) and server data sources.
    /// On Initialize: loads from local CatalogConfig SO.
    /// TryLoadFromServer: attempts to replace data with server catalog.
    /// </summary>
    public class CatalogService : ICatalogService
    {
        private const string CatalogConfigKey = "CatalogConfig";

        private CatalogConfig _config;
        private Dictionary<string, ItemData> _itemsById;
        private Dictionary<ItemType, List<ItemData>> _itemsByType;
        private Dictionary<ItemRarity, List<ItemData>> _itemsByRarity;

        /// <summary>Runtime-created ItemData instances (from server). Tracked for cleanup.</summary>
        private readonly List<ItemData> _runtimeItems = new List<ItemData>();

        /// <summary>Whether current data came from the server.</summary>
        public bool IsServerData { get; private set; }

        public int InitializationOrder => 20;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;

            var configService = ServiceLocator.Get<IConfigService>();
            _config = configService.GetConfig<CatalogConfig>(CatalogConfigKey);

            BuildIndexes(_config?.Items);

            IsInitialized = true;
            IsServerData = false;
            OnCatalogLoaded?.Invoke();

            Debug.Log($"<color=#00AA00>[CatalogService] Initialized with {ItemCount} items from local SO</color>");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;

            _itemsById?.Clear();
            _itemsByType?.Clear();
            _itemsByRarity?.Clear();
            CleanupRuntimeItems();
            _config = null;
            IsInitialized = false;
        }

        public event Action OnCatalogLoaded;

        public int ItemCount => _itemsById?.Count ?? 0;

        public IReadOnlyList<ItemData> GetAllItems()
        {
            if (_itemsById == null) return new List<ItemData>();
            return new List<ItemData>(_itemsById.Values);
        }

        public ItemData GetItem(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_itemsById != null && _itemsById.TryGetValue(id, out var item))
                return item;
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
                return items;
            return new List<ItemData>();
        }

        public IReadOnlyList<ItemData> GetItemsByRarity(ItemRarity rarity)
        {
            if (_itemsByRarity != null && _itemsByRarity.TryGetValue(rarity, out var items))
                return items;
            return new List<ItemData>();
        }

        public IReadOnlyList<ItemData> GetItems(Predicate<ItemData> filter)
        {
            if (filter == null || _itemsById == null) return new List<ItemData>();
            var result = new List<ItemData>();
            foreach (var item in _itemsById.Values)
            {
                if (item != null && filter(item))
                    result.Add(item);
            }
            return result;
        }

        public bool HasItem(string id)
        {
            return !string.IsNullOrEmpty(id) && _itemsById != null && _itemsById.ContainsKey(id);
        }

        public void ReloadCatalog()
        {
            if (IsServerData && _runtimeItems.Count > 0)
                BuildIndexesFromRuntime();
            else
                BuildIndexes(_config?.Items);
            OnCatalogLoaded?.Invoke();
        }

        // ────────────────────────────────────────────
        //  Server Integration
        // ────────────────────────────────────────────

        /// <summary>
        /// Attempt to load the catalog from the server.
        /// On success: replaces local data with server data.
        /// On failure: logs error, keeps existing local data.
        /// </summary>
        public IEnumerator TryLoadFromServer(Action<bool> onComplete)
        {
            if (!ServiceLocator.TryGet<ICoreServerService>(out var coreServer) || !coreServer.IsAuthenticated)
            {
                Debug.LogWarning("[CatalogService] Cannot load from server: not authenticated.");
                onComplete?.Invoke(false);
                yield break;
            }

            bool done = false;
            bool success = false;

            yield return coreServer.GetCatalog(
                catalog =>
                {
                    if (catalog?.items != null && catalog.items.Count > 0)
                    {
                        ApplyServerCatalog(catalog);
                        success = true;
                        Debug.Log($"<color=#00FF00>[CatalogService] Loaded {ItemCount} items from server</color>");
                    }
                    else
                    {
                        Debug.LogWarning("[CatalogService] Server returned empty catalog. Keeping local data.");
                    }
                    done = true;
                },
                error =>
                {
                    Debug.LogError($"[CatalogService] Failed to load catalog from server: {error}");
                    done = true;
                });

            yield return new WaitUntil(() => done);
            onComplete?.Invoke(success);
        }

        /// <summary>
        /// Convert server catalog DTO into runtime ItemData instances and replace current data.
        /// </summary>
        private void ApplyServerCatalog(CatalogResponse catalog)
        {
            CleanupRuntimeItems();

            foreach (var template in catalog.items)
            {
                if (template.variants == null) continue;

                var itemType = ItemTypeExtensions.FromServerSlot(template.slot);

                foreach (var variant in template.variants)
                {
                    var itemData = ScriptableObject.CreateInstance<ItemData>();
                    itemData.name = $"{template.code}_{variant.rarity}";

                    var levels = ConvertLevels(variant.levels);
                    var bonuses = ConvertBonuses(variant.bonuses);
                    var rarity = ItemRarityExtensions.FromServerString(variant.rarity);
                    var mainStat = StatTypeExtensions.FromServerString(variant.mainStatType);

#if UNITY_EDITOR
                    itemData.PopulateFromServer(
                        template.id, template.code, template.name, template.description,
                        itemType, variant.id, rarity, mainStat, variant.maxLevel,
                        levels, bonuses);
#endif
                    // For runtime (non-editor), we use reflection or a runtime populate method
                    // Since PopulateFromServer is editor-only, we provide a runtime path:
                    PopulateItemDataRuntime(itemData, template, variant, itemType, rarity, mainStat, levels, bonuses);

                    _runtimeItems.Add(itemData);
                }
            }

            BuildIndexesFromRuntime();
            IsServerData = true;
            OnCatalogLoaded?.Invoke();
        }

        private void PopulateItemDataRuntime(ItemData itemData,
            CatalogItemDTO template, CatalogVariantDTO variant,
            ItemType itemType, ItemRarity rarity, StatType mainStat,
            List<ItemLevelData> levels, List<ItemBonusData> bonuses)
        {
            // Use reflection to set private fields at runtime
            var type = typeof(ItemData);
            SetField(type, itemData, "_id", variant.id);
            SetField(type, itemData, "_templateId", template.id);
            SetField(type, itemData, "_code", template.code);
            SetField(type, itemData, "_displayName", template.name);
            SetField(type, itemData, "_description", template.description);
            SetField(type, itemData, "_itemType", itemType);
            SetField(type, itemData, "_rarity", rarity);
            SetField(type, itemData, "_mainStatType", mainStat);
            SetField(type, itemData, "_maxLevel", variant.maxLevel);
            SetField(type, itemData, "_levels", levels);
            SetField(type, itemData, "_bonuses", bonuses);
            SetField(type, itemData, "_perTapBonus", levels.Count > 0 ? levels[0].Value : 0f);
        }

        private static void SetField(Type type, object obj, string fieldName, object value)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(obj, value);
        }

        private static List<ItemLevelData> ConvertLevels(List<CatalogLevelDTO> dtos)
        {
            var result = new List<ItemLevelData>();
            if (dtos == null) return result;
            foreach (var dto in dtos)
            {
                result.Add(new ItemLevelData
                {
                    Level = dto.level,
                    Value = dto.value,
                    ValuePercent = dto.valuePercent,
                });
            }
            return result;
        }

        private static List<ItemBonusData> ConvertBonuses(List<CatalogBonusDTO> dtos)
        {
            var result = new List<ItemBonusData>();
            if (dtos == null) return result;
            foreach (var dto in dtos)
            {
                result.Add(new ItemBonusData
                {
                    StatType = StatTypeExtensions.FromServerString(dto.statType),
                    Value = dto.value,
                    ValuePercent = dto.valuePercent,
                    Description = dto.description,
                });
            }
            return result;
        }

        // ────────────────────────────────────────────
        //  Index Building
        // ────────────────────────────────────────────

        private void BuildIndexes(IReadOnlyList<ItemData> items)
        {
            _itemsById = new Dictionary<string, ItemData>();
            _itemsByType = new Dictionary<ItemType, List<ItemData>>();
            _itemsByRarity = new Dictionary<ItemRarity, List<ItemData>>();

            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;
                IndexItem(item);
            }
        }

        private void BuildIndexesFromRuntime()
        {
            _itemsById = new Dictionary<string, ItemData>();
            _itemsByType = new Dictionary<ItemType, List<ItemData>>();
            _itemsByRarity = new Dictionary<ItemRarity, List<ItemData>>();

            foreach (var item in _runtimeItems)
            {
                if (item == null) continue;
                IndexItem(item);
            }
        }

        private void IndexItem(ItemData item)
        {
            // Index by ID
            if (!string.IsNullOrEmpty(item.Id))
            {
                if (_itemsById.ContainsKey(item.Id))
                    Debug.LogWarning($"[CatalogService] Duplicate item ID: {item.Id}");
                else
                    _itemsById[item.Id] = item;
            }

            // Index by Type
            if (!_itemsByType.ContainsKey(item.ItemType))
                _itemsByType[item.ItemType] = new List<ItemData>();
            _itemsByType[item.ItemType].Add(item);

            // Index by Rarity
            if (!_itemsByRarity.ContainsKey(item.Rarity))
                _itemsByRarity[item.Rarity] = new List<ItemData>();
            _itemsByRarity[item.Rarity].Add(item);
        }

        private void CleanupRuntimeItems()
        {
            foreach (var item in _runtimeItems)
            {
                if (item != null)
                    UnityEngine.Object.Destroy(item);
            }
            _runtimeItems.Clear();
        }
    }
}
