using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Core.Inventory;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIModel : UIBaseModel
    {
        public ReactiveProperty<int> HitsCurrent { get; private set; }
        public ReactiveProperty<int> HitsMax { get; private set; }
        public ReactiveProperty<int> CoinsPerTap { get; private set; }
        public ReactiveProperty<float> TotalEquipmentBonus { get; private set; }
        public ReactiveProperty<bool> IsRefreshing { get; private set; }
        
        private IPlayerService _playerService;
        private ITapControllerService _tapController;
        private IInventoryService _inventoryService;

        public IInventoryService InventoryService => _inventoryService;

        public override void Initialize()
        {
            base.Initialize();
            
            HitsCurrent = new ReactiveProperty<int>(0);
            HitsMax = new ReactiveProperty<int>(0);
            CoinsPerTap = new ReactiveProperty<int>(1);
            TotalEquipmentBonus = new ReactiveProperty<float>(0f);
            IsRefreshing = new ReactiveProperty<bool>(false);
            
            _playerService = ServiceLocator.Get<IPlayerService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            
            if (ServiceLocator.TryGet(out _inventoryService))
            {
                _inventoryService.OnItemEquipChanged += OnItemEquipChanged;
                RecalculateTotalBonus();
            }
            
            if (_tapController != null)
            {
                _tapController.OnHitsChanged += OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged += OnIncomeMultiplierChanged;
                
                HitsCurrent.Value = _tapController.CurrentHits;
                HitsMax.Value = _tapController.MaxHits;
                RecalculateCoinsPerTap();
            }
            
            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
            }
        }

        private void OnHitsChanged(int current, int max)
        {
            HitsCurrent.Value = current;
            HitsMax.Value = max;
        }

        private void OnIncomeMultiplierChanged(float multiplier)
        {
            RecalculateCoinsPerTap();
        }

        private void OnPlayerDataChanged(PlayerData playerData)
        {
            RecalculateCoinsPerTap();
        }
        
        private void OnItemEquipChanged(InventoryItem item, bool isEquipped)
        {
            RecalculateTotalBonus();
        }

        private void RecalculateCoinsPerTap()
        {
            if (_playerService == null) return;
            var baseIncome = _playerService.IncomePerTap;
            var multiplier = _playerService.IncomeMultiplier;
            var effective = Mathf.Max(0, Mathf.RoundToInt(baseIncome * multiplier));
            CoinsPerTap.Value = effective;
        }
        
        private void RecalculateTotalBonus()
        {
            if (_inventoryService == null) return;
            
            float total = 0f;
            var equippedItems = _inventoryService.GetEquippedItems();
            foreach (var item in equippedItems)
            {
                if (item?.Data != null)
                {
                    total += item.Data.PerTapBonus;
                }
            }
            TotalEquipmentBonus.Value = total;
        }
        
        /// <summary>
        /// Get all inventory items.
        /// </summary>
        public IReadOnlyList<InventoryItem> GetAllItems()
        {
            return _inventoryService?.Items ?? new List<InventoryItem>();
        }
        
        /// <summary>
        /// Get equipped items.
        /// </summary>
        public IReadOnlyList<InventoryItem> GetEquippedItems()
        {
            return _inventoryService?.GetEquippedItems() ?? new List<InventoryItem>();
        }
        
        /// <summary>
        /// Equip an item.
        /// </summary>
        public bool EquipItem(string instanceId)
        {
            return _inventoryService?.EquipItem(instanceId) ?? false;
        }
        
        /// <summary>
        /// Unequip an item.
        /// </summary>
        public bool UnequipItem(string instanceId)
        {
            return _inventoryService?.UnequipItem(instanceId) ?? false;
        }

        /// <summary>
        /// Refresh inventory from server. Uses InventoryService.TryLoadFromServer
        /// which replaces local data on success and fires OnInventoryLoaded.
        /// </summary>
        public IEnumerator RefreshFromServer(Action<bool> onComplete = null)
        {
            if (_inventoryService is not InventoryService invService)
            {
                Debug.LogWarning("[InventoryScreenUIModel] InventoryService not available for server refresh");
                onComplete?.Invoke(false);
                yield break;
            }

            if (!ServiceLocator.TryGet<ICoreServerService>(out var coreService) || !coreService.IsAuthenticated)
            {
                Debug.LogWarning("[InventoryScreenUIModel] Not authenticated — skipping server refresh");
                onComplete?.Invoke(false);
                yield break;
            }

            IsRefreshing.Value = true;
            Debug.Log("<color=#00AAFF>[InventoryScreenUIModel] Refreshing inventory from server...</color>");

            yield return invService.TryLoadFromServer(success =>
            {
                IsRefreshing.Value = false;
                if (success)
                {
                    RecalculateTotalBonus();
                    Debug.Log("<color=#00FF00>[InventoryScreenUIModel] Inventory refreshed from server</color>");
                }
                else
                {
                    Debug.LogWarning("[InventoryScreenUIModel] Server refresh failed — keeping local data");
                }
                onComplete?.Invoke(success);
            });
        }

        public override void Dispose()
        {
            if (_tapController != null)
            {
                _tapController.OnHitsChanged -= OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged -= OnIncomeMultiplierChanged;
            }
            
            if (_playerService != null)
            {
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
            }
            
            if (_inventoryService != null)
            {
                _inventoryService.OnItemEquipChanged -= OnItemEquipChanged;
            }
            
            HitsCurrent?.Dispose();
            HitsMax?.Dispose();
            CoinsPerTap?.Dispose();
            TotalEquipmentBonus?.Dispose();
            IsRefreshing?.Dispose();
            
            base.Dispose();
        }
    }
}
