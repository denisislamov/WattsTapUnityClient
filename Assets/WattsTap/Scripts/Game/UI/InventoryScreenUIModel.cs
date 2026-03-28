using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Core.Configs;
using WattsTap.Core.Inventory;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.UI
{
    public class InventoryScreenUIModel : UIBaseModel
    {
        public ReactiveProperty<int> HitsCurrent { get; private set; }
        public ReactiveProperty<int> HitsMax { get; private set; }
        public ReactiveProperty<int> CoinsPerTap { get; private set; }
        public ReactiveProperty<float> TotalEquipmentBonus { get; private set; }
        public ReactiveProperty<bool> IsRefreshing { get; private set; }
        
        // Per-stat bonuses from equipped items
        public ReactiveProperty<float> BonusCoinsPerTap { get; private set; }
        public ReactiveProperty<float> BonusXpPerTap { get; private set; }
        public ReactiveProperty<float> BonusCapacityHits { get; private set; }
        public ReactiveProperty<float> BonusRecoverySpeed { get; private set; }
        public ReactiveProperty<float> BonusCritChance { get; private set; }
        public ReactiveProperty<float> BonusCritMultiplier { get; private set; }
        public ReactiveProperty<float> BonusOfflinePercent { get; private set; }
        
        /// <summary>Total profit per tap (base + equipment) × multiplier.</summary>
        public ReactiveProperty<int> ProfitPerTap { get; private set; }
        
        /// <summary>Total profit per hour (base passive + equipment offline bonus).</summary>
        public ReactiveProperty<int> ProfitPerHour { get; private set; }
        
        /// <summary>Total recovery speed (base + equipment bonus), hits per second.</summary>
        public ReactiveProperty<float> TotalRecovery { get; private set; }
        
        private IPlayerService _playerService;
        private ITapControllerService _tapController;
        private IInventoryService _inventoryService;
        private MiningBalanceConfig _miningBalanceConfig;

        public IInventoryService InventoryService => _inventoryService;

        public override void Initialize()
        {
            base.Initialize();
            
            HitsCurrent = new ReactiveProperty<int>(0);
            HitsMax = new ReactiveProperty<int>(0);
            CoinsPerTap = new ReactiveProperty<int>(1);
            TotalEquipmentBonus = new ReactiveProperty<float>(0f);
            IsRefreshing = new ReactiveProperty<bool>(false);
            
            BonusCoinsPerTap = new ReactiveProperty<float>(0f);
            BonusXpPerTap = new ReactiveProperty<float>(0f);
            BonusCapacityHits = new ReactiveProperty<float>(0f);
            BonusRecoverySpeed = new ReactiveProperty<float>(0f);
            BonusCritChance = new ReactiveProperty<float>(0f);
            BonusCritMultiplier = new ReactiveProperty<float>(0f);
            BonusOfflinePercent = new ReactiveProperty<float>(0f);
            ProfitPerTap = new ReactiveProperty<int>(0);
            ProfitPerHour = new ReactiveProperty<int>(0);
            TotalRecovery = new ReactiveProperty<float>(0f);
            
            _playerService = ServiceLocator.Get<IPlayerService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            
            var configService = ServiceLocator.Get<IConfigService>();
            _miningBalanceConfig = configService?.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
            
            if (ServiceLocator.TryGet(out _inventoryService))
            {
                _inventoryService.OnItemEquipChanged += OnItemEquipChanged;
                RecalculateEquippedStats();
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
            RecalculateEquippedStats();
        }

        private void RecalculateCoinsPerTap()
        {
            if (_playerService == null) return;
            var baseIncome = _playerService.IncomePerTap;
            var multiplier = _playerService.IncomeMultiplier;
            var equipBonus = BonusCoinsPerTap?.Value ?? 0f;
            var effective = Mathf.Max(0, Mathf.RoundToInt((baseIncome + equipBonus) * multiplier));
            CoinsPerTap.Value = effective;
        }
        
        private void RecalculateEquippedStats()
        {
            if (_inventoryService == null) return;
            
            var bonuses = EquippedStatsCalculator.Calculate(_inventoryService);
            
            var coins = EquippedStatsCalculator.GetBonus(bonuses, StatType.CoinsPerTap);
            var xp = EquippedStatsCalculator.GetBonus(bonuses, StatType.XpPerTap);
            var capacity = EquippedStatsCalculator.GetBonus(bonuses, StatType.CapacityHits);
            var recovery = EquippedStatsCalculator.GetBonus(bonuses, StatType.RecoverHitsPerSecond);
            var critChance = EquippedStatsCalculator.GetBonus(bonuses, StatType.CritChance);
            var critMult = EquippedStatsCalculator.GetBonus(bonuses, StatType.CritMultiplier);
            var offline = EquippedStatsCalculator.GetBonus(bonuses, StatType.OfflineBonusPercent);
            
            BonusCoinsPerTap.Value = coins.Flat;
            BonusXpPerTap.Value = xp.Flat;
            BonusCapacityHits.Value = capacity.Flat;
            BonusRecoverySpeed.Value = recovery.Flat;
            BonusCritChance.Value = critChance.Percent > 0 ? critChance.Percent : critChance.Flat;
            BonusCritMultiplier.Value = critMult.Percent > 0 ? critMult.Percent : critMult.Flat;
            BonusOfflinePercent.Value = offline.Percent > 0 ? offline.Percent : offline.Flat;
            
            // Legacy total bonus (sum of all flat values)
            float total = 0f;
            foreach (var kvp in bonuses)
                total += kvp.Value.Flat;
            TotalEquipmentBonus.Value = total;
            
            // Recalculate displayed CoinsPerTap because equipment bonus changed
            RecalculateCoinsPerTap();
            RecalculateSummaryStats(bonuses);
        }
        
        private void RecalculateSummaryStats(Dictionary<StatType, EquippedStatsCalculator.StatBonus> bonuses)
        {
            // --- Profit Per Tap ---
            if (_playerService != null)
            {
                var baseIncome = _playerService.IncomePerTap;
                var multiplier = _playerService.IncomeMultiplier;
                var coinsBonus = EquippedStatsCalculator.GetBonus(bonuses, StatType.CoinsPerTap);
                var tap = Mathf.RoundToInt((baseIncome + coinsBonus.Flat) * multiplier * (1f + coinsBonus.Percent / 100f));
                ProfitPerTap.Value = Mathf.Max(0, tap);
            }
            
            // --- Profit Per Hour ---
            {
                var baseProfitPerHour = _miningBalanceConfig != null ? _miningBalanceConfig.profitPerHour : 0;
                var offlineBonus = EquippedStatsCalculator.GetBonus(bonuses, StatType.OfflineBonusPercent);
                var hour = Mathf.RoundToInt(baseProfitPerHour * (1f + offlineBonus.Percent / 100f) + offlineBonus.Flat);
                ProfitPerHour.Value = Mathf.Max(0, hour);
            }
            
            // --- Total Recovery (hits per second) ---
            {
                var baseRecovery = _miningBalanceConfig != null ? 1f / _miningBalanceConfig.cooldownPerHitSec : 0f;
                var recoveryBonus = EquippedStatsCalculator.GetBonus(bonuses, StatType.RecoverHitsPerSecond);
                TotalRecovery.Value = baseRecovery + recoveryBonus.Flat;
            }
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
                    RecalculateEquippedStats();
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
            
            BonusCoinsPerTap?.Dispose();
            BonusXpPerTap?.Dispose();
            BonusCapacityHits?.Dispose();
            BonusRecoverySpeed?.Dispose();
            BonusCritChance?.Dispose();
            BonusCritMultiplier?.Dispose();
            BonusOfflinePercent?.Dispose();
            ProfitPerTap?.Dispose();
            ProfitPerHour?.Dispose();
            TotalRecovery?.Dispose();
            
            base.Dispose();
        }
    }
}
