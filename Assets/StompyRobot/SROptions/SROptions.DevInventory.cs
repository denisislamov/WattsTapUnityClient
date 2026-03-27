using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using SRDebugger;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Core.Inventory;

public partial class SROptions
{
    private string _devItemVariantId = "";
    private int _devItemLevel = 1;
    private int _devItemCatalogIndex;
    private List<string> _cachedVariantIds;

    #region Properties

    [Category("Dev Inventory")]
    [DisplayName("Item Variant ID")]
    [Sort(0)]
    public string DevItemVariantId
    {
        get => _devItemVariantId;
        set
        {
            _devItemVariantId = value ?? "";
            OnPropertyChanged("DevItemVariantId");
        }
    }

    [Category("Dev Inventory")]
    [DisplayName("Item Level")]
    [Increment(1)]
    [NumberRange(1, 10)]
    [Sort(1)]
    public int DevItemLevel
    {
        get => _devItemLevel;
        set
        {
            _devItemLevel = Mathf.Clamp(value, 1, 10);
            OnPropertyChanged("DevItemLevel");
        }
    }

    [Category("Dev Inventory")]
    [DisplayName("Catalog Index")]
    [Increment(1)]
    [Sort(2)]
    public int DevItemCatalogIndex
    {
        get => _devItemCatalogIndex;
        set
        {
            var ids = GetCachedVariantIds();
            _devItemCatalogIndex = ids.Count > 0 ? Mathf.Clamp(value, 0, ids.Count - 1) : 0;
            if (ids.Count > 0)
                _devItemVariantId = ids[_devItemCatalogIndex];
            OnPropertyChanged("DevItemCatalogIndex");
            OnPropertyChanged("DevItemVariantId");
        }
    }

    #endregion

    #region Actions

    [Category("Dev Inventory")]
    [DisplayName("▶ Grant Item")]
    [Sort(3)]
    public void DevGrantItemAction()
    {
        if (string.IsNullOrEmpty(_devItemVariantId))
        {
            Debug.LogWarning("[SROptions] Item Variant ID must not be empty");
            return;
        }

        SendDevGrantItem(_devItemVariantId, _devItemLevel);
    }

    [Category("Dev Inventory")]
    [DisplayName("📋 Log Catalog Items")]
    [Sort(4)]
    public void DevLogCatalogItems()
    {
        _cachedVariantIds = null; // force refresh
        var ids = GetCachedVariantIds();

        if (ids.Count == 0)
        {
            Debug.LogWarning("[SROptions] Catalog is empty or CatalogService unavailable");
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"<color=#00FFFF>[SROptions] === Catalog Items ({ids.Count}) ===</color>");

        if (ServiceLocator.TryGet<ICatalogService>(out var catalog))
        {
            int i = 0;
            foreach (var id in ids)
            {
                var item = catalog.GetItem(id);
                if (item != null)
                    sb.AppendLine($"  [{i}] {item.DisplayName} ({item.Rarity}) — type:{item.ItemType} — id:{id}");
                else
                    sb.AppendLine($"  [{i}] id:{id}");
                i++;
            }
        }

        Debug.Log(sb.ToString());
    }

    [Category("Dev Inventory")]
    [DisplayName("🔄 Reload Inventory from Server")]
    [Sort(5)]
    public void DevReloadInventory()
    {
        if (!ServiceLocator.TryGet<ICoreServerService>(out var coreService) || !coreService.IsAuthenticated)
        {
            Debug.LogError("[SROptions] Not authenticated — cannot reload inventory");
            return;
        }

        var runner = GetCoroutineRunner();
        if (runner == null)
        {
            Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
            return;
        }

        Debug.Log("<color=#FF00FF>[SROptions] Reloading inventory from server...</color>");
        runner.StartCoroutine(DevReloadInventoryCoroutine(coreService));
    }

    #endregion

    #region Private Helpers

    private void SendDevGrantItem(string itemVariantId, int level)
    {
        if (!ServiceLocator.TryGet<ICoreServerService>(out var coreService) || !coreService.IsAuthenticated)
        {
            Debug.LogError("[SROptions] Not authenticated — cannot grant item");
            return;
        }

        var runner = GetCoroutineRunner();
        if (runner == null)
        {
            Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
            return;
        }

        Debug.Log($"<color=#FF00FF>[SROptions] Granting item: variantId={itemVariantId}, level={level}</color>");
        runner.StartCoroutine(DevGrantItemCoroutine(coreService, itemVariantId, level));
    }

    private IEnumerator DevGrantItemCoroutine(ICoreServerService coreService, string itemVariantId, int level)
    {
        yield return coreService.DevGrantInventoryItem(
            itemVariantId, level,
            onSuccess: response =>
            {
                Debug.Log($"<color=#00FF00>[SROptions] Item granted! playerItemId={response.playerItemId}, variantId={response.itemVariantId}, level={response.level}</color>");
            },
            onError: error =>
            {
                Debug.LogError($"[SROptions] Failed to grant item: {error}");
            }
        );
    }

    private IEnumerator DevReloadInventoryCoroutine(ICoreServerService coreService)
    {
        yield return coreService.GetInventory(
            onSuccess: response =>
            {
                Debug.Log($"<color=#00FF00>[SROptions] Inventory reloaded: {response.inventory?.Count ?? 0} items</color>");
            },
            onError: error =>
            {
                Debug.LogError($"[SROptions] Failed to reload inventory: {error}");
            }
        );
    }

    private List<string> GetCachedVariantIds()
    {
        if (_cachedVariantIds != null) return _cachedVariantIds;

        _cachedVariantIds = new List<string>();

        if (ServiceLocator.TryGet<ICatalogService>(out var catalog))
        {
            var items = catalog.GetAllItems();
            foreach (var item in items)
            {
                if (item != null && !string.IsNullOrEmpty(item.Id))
                    _cachedVariantIds.Add(item.Id);
            }
        }

        return _cachedVariantIds;
    }

    #endregion
}


