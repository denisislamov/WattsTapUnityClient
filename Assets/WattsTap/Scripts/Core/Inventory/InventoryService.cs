using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core.API;
using WattsTap.Core.Configs;
using WattsTap.Core.Configs.CoreServer;

namespace WattsTap.Core.Inventory
{
    /// <summary>
    /// Service for managing player's inventory.
    /// Supports both local (InventoryStartConfig SO) and server data sources.
    /// On Initialize: loads from local SO.
    /// TryLoadFromServer: replaces with server data or keeps local on failure.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private const string StartConfigKey = "InventoryStartConfig";

        private readonly List<InventoryItem> _items = new List<InventoryItem>();
        private ICatalogService _catalogService;

        /// <summary>Whether current data came from the server.</summary>
        public bool IsServerData { get; private set; }

        /// <summary>Player currencies from server.</summary>
        public long Coins { get; private set; }
        public long Drawings { get; private set; }

        /// <summary>Whether inventory start config is in debug mode (use local data instead of server).</summary>
        public bool IsDebugMode { get; private set; }

        public int InitializationOrder => 25; // After CatalogService (20)
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;

            _catalogService = ServiceLocator.Get<ICatalogService>();

            // Try to load start config and check debug mode
            var configService = ServiceLocator.Get<IConfigService>();
            if (configService != null)
            {
                try
                {
                    var startConfig = configService.GetConfig<InventoryStartConfig>(StartConfigKey);
                    if (startConfig != null)
                    {
                        IsDebugMode = startConfig.IsDebug;
                        
                        if (startConfig.IsDebug)
                        {
                            // Debug mode: load inventory from local SO config
                            IsInitialized = true; // Set before applying to allow AddItem to work
                            startConfig.ApplyToInventory(this);
                            Debug.Log("<color=#FFAA00>[InventoryService] Debug mode ON — using local SO inventory</color>");
                        }
                        else
                        {
                            Debug.Log("<color=#00AA00>[InventoryService] Debug mode OFF — inventory will be loaded from server</color>");
                        }
                    }
                }
                catch
                {
                    // Start config not registered, that's okay
                }
            }

            IsInitialized = true;
            IsServerData = false;
            Debug.Log($"<color=#00AA00>[InventoryService] Initialized (debugMode={IsDebugMode})</color>");
        }

        public void Shutdown()
        {
            if (!IsInitialized) return;

            _items.Clear();
            _catalogService = null;
            IsInitialized = false;
        }

        // ────────────────────────────────────────────
        //  IInventoryService
        // ────────────────────────────────────────────

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
            if (string.IsNullOrEmpty(itemId)) return false;
            if (!_catalogService.TryGetItem(itemId, out var itemData))
            {
                Debug.LogWarning($"[InventoryService] Item not found in catalog: {itemId}");
                return false;
            }
            return AddItem(itemData);
        }

        public bool AddItem(ItemData itemData)
        {
            if (itemData == null) return false;
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
                if (item.Data?.Id == itemId)
                    return true;
            return false;
        }

        public IReadOnlyList<InventoryItem> GetItemsById(string itemId)
        {
            var result = new List<InventoryItem>();
            foreach (var item in _items)
                if (item.Data?.Id == itemId)
                    result.Add(item);
            return result;
        }

        public IReadOnlyList<InventoryItem> GetItemsByType(ItemType type)
        {
            var result = new List<InventoryItem>();
            foreach (var item in _items)
                if (item.Data?.ItemType == type)
                    result.Add(item);
            return result;
        }

        public InventoryItem GetItemByInstanceId(string instanceId)
        {
            foreach (var item in _items)
                if (item.InstanceId == instanceId)
                    return item;
            return null;
        }

        public IReadOnlyList<InventoryItem> GetEquippedItems()
        {
            var result = new List<InventoryItem>();
            foreach (var item in _items)
                if (item.IsEquipped)
                    result.Add(item);
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
                saveData.Items.Add(new InventoryItemSaveData(item));
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
                    itemSave.InstanceId, itemData,
                    new DateTime(itemSave.AcquiredAtTicks), itemSave.IsEquipped);
                _items.Add(item);
            }
            OnInventoryLoaded?.Invoke();
            Debug.Log($"<color=#00AA00>[InventoryService] Loaded {_items.Count} items from save data</color>");
        }

        // ────────────────────────────────────────────
        //  Server Integration
        // ────────────────────────────────────────────

        /// <summary>
        /// Attempt to load inventory from server.
        /// On success: replaces current inventory with server data.
        /// On failure: logs error, keeps existing local data.
        /// </summary>
        public IEnumerator TryLoadFromServer(Action<bool> onComplete)
        {
            if (!ServiceLocator.TryGet<ICoreServerService>(out var coreServer) || !coreServer.IsAuthenticated)
            {
                Debug.LogWarning("[InventoryService] Cannot load from server: not authenticated.");
                onComplete?.Invoke(false);
                yield break;
            }

            // Log CURL equivalent
            var baseUrl = GetBaseUrlForLog();
            var token = coreServer.AuthToken;
            Debug.Log($"<color=#FFA500>[CURL] curl -X GET '{baseUrl}/game/inventory' \\\n" +
                      $"  -H 'Authorization: Bearer {token}'</color>");

            bool done = false;
            bool success = false;

            yield return coreServer.GetInventory(
                response =>
                {
                    if (response != null)
                    {
                        var responseJson = JsonUtility.ToJson(response, true);
                        Debug.Log($"<color=#00FF00>[CURL] ← Response (Get Inventory):\n{responseJson}</color>");

                        ApplyServerInventory(response);
                        success = true;
                        Debug.Log($"<color=#00FF00>[InventoryService] Loaded {_items.Count} items from server (coins={Coins})</color>");
                    }
                    else
                    {
                        Debug.LogWarning("[CURL] ← Response (Get Inventory): null");
                        Debug.LogWarning("[InventoryService] Server returned null inventory. Keeping local data.");
                    }
                    done = true;
                },
                error =>
                {
                    Debug.LogError($"[CURL] ← Error (Get Inventory): {error}");
                    Debug.LogError($"[InventoryService] Failed to load inventory from server: {error}");
                    done = true;
                });

            yield return new WaitUntil(() => done);
            onComplete?.Invoke(success);
        }

        private void ApplyServerInventory(InventoryResponse response)
        {
            _items.Clear();

            // Apply currencies
            if (response.currencies != null)
            {
                Coins = response.currencies.coins;
                Drawings = response.currencies.drawings;
            }

            // Convert server inventory items
            if (response.inventory != null)
            {
                foreach (var dto in response.inventory)
                {
                    if (string.IsNullOrEmpty(dto.variantId)) continue;

                    // Find ItemData in catalog by variantId
                    if (!_catalogService.TryGetItem(dto.variantId, out var itemData))
                    {
                        Debug.LogWarning($"[InventoryService] Variant '{dto.variantId}' not found in catalog, skipping");
                        continue;
                    }

                    var invItem = new InventoryItem(dto.id, itemData, dto.level, dto.isEquipped);
                    _items.Add(invItem);
                }
            }

            IsServerData = true;
            OnInventoryLoaded?.Invoke();
        }

        private string GetBaseUrlForLog()
        {
            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                try
                {
                    var cfg = configService.GetConfig<CoreServerConfig>("CoreServerConfig");
                    if (cfg != null) return cfg.BaseUrl;
                }
                catch { /* fallback */ }
            }
            return "https://api-dev.wattstap.energy";
        }
    }
}
