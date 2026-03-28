using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Core.Inventory;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.UI
{
    public class MainMenuUIModel : UIBaseModel
    {
        public ReactiveProperty<string> PlayerName { get; private set; }
        public ReactiveProperty<int> PlayerLevel { get; private set; }
        public ReactiveProperty<bool> IsLoading { get; private set; }
        public ReactiveProperty<long> TotalCoins { get; private set; }
        public ReactiveProperty<int> CoinsPerTap { get; private set; }
        public ReactiveProperty<int> HitsCurrent { get; private set; }
        public ReactiveProperty<int> HitsMax { get; private set; }
        
        public ReactiveProperty<int> Level { get; private set; }
        public ReactiveProperty<long> CurrentXp { get; private set; }
        public ReactiveProperty<long> XpToNextLevel { get; private set; }
        
        /// <summary>Total profit per tap (base + equipment bonuses) × multiplier.</summary>
        public ReactiveProperty<int> ProfitPerTap { get; private set; }
        
        /// <summary>Total profit per hour (base passive + equipment offline bonus).</summary>
        public ReactiveProperty<int> ProfitPerHour { get; private set; }
        
        // Equipment bonus shown alongside CoinsPerTap
        public ReactiveProperty<float> EquipBonusCoinsPerTap { get; private set; }
        
        private IPlayerService _playerService;
        private ITapControllerService _tapController;
        private IInventoryService _inventoryService;
        private MiningBalanceConfig _miningBalanceConfig;

        public override void Initialize()
        {
            base.Initialize();
            
            PlayerName = new ReactiveProperty<string>("Player");
            PlayerLevel = new ReactiveProperty<int>(1);
            IsLoading = new ReactiveProperty<bool>(false);
            TotalCoins = new ReactiveProperty<long>(0);
            CoinsPerTap = new ReactiveProperty<int>(1);
            HitsCurrent = new ReactiveProperty<int>(0);
            HitsMax = new ReactiveProperty<int>(0);
            Level = new ReactiveProperty<int>(1);
            CurrentXp = new ReactiveProperty<long>(0);
            XpToNextLevel = new ReactiveProperty<long>(100);
            EquipBonusCoinsPerTap = new ReactiveProperty<float>(0f);
            ProfitPerTap = new ReactiveProperty<int>(0);
            ProfitPerHour = new ReactiveProperty<int>(0);
            
            // Получаем сервис игрока
            _playerService = ServiceLocator.Get<IPlayerService>();
            _tapController = ServiceLocator.Get<ITapControllerService>();
            ServiceLocator.TryGet(out _inventoryService);
            
            var configService = ServiceLocator.Get<IConfigService>();
            _miningBalanceConfig = configService?.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
            
            if (_inventoryService != null)
            {
                _inventoryService.OnItemEquipChanged += OnItemEquipChanged;
                _inventoryService.OnInventoryLoaded += OnInventoryLoaded;
                RecalculateEquipBonus();
            }
            
            if (_playerService != null)
            {
                _playerService.OnResourcesChanged += OnPlayerResourcesChanged;
                _playerService.OnPlayerDataChanged += OnPlayerDataChanged;
                _playerService.OnLevelUp += OnLevelUp;
                
                UpdateFromPlayerData();
            }

            if (_tapController != null)
            {
                _tapController.OnHitsChanged += OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged += OnIncomeMultiplierChanged;
                // Инициализируем текущие значения хитов
                HitsCurrent.Value = _tapController.CurrentHits;
                HitsMax.Value = _tapController.MaxHits;
                // Инициализируем CoinsPerTap с учётом множителя
                RecalculateCoinsPerTap();
            }
        }

        private void OnLevelUp(int value)
        {
            Level.Value = value;
        }

        private void OnPlayerResourcesChanged(PlayerResources resources)
        {
            TotalCoins.Value = resources.watts;
            // Update XpToNextLevel BEFORE CurrentXp to ensure correct progress calculation
            XpToNextLevel.Value = resources.xpToNextLevel;
            CurrentXp.Value = resources.currentXP;
        }

        private void OnPlayerDataChanged(PlayerData playerData)
        {
            UpdateFromPlayerData();
            // База дохода за тап могла измениться — пересчитаем отображение
            RecalculateCoinsPerTap();
        }

        private void UpdateFromPlayerData()
        {
            if (_playerService == null) return;
            
            var playerData = _playerService.GetPlayerData();
            if (playerData != null)
            {
                TotalCoins.Value = playerData.resources.watts;
                // CoinsPerTap будет рассчитываться отдельно с учётом множителя
                // PlayerName.Value = playerData.nickname;
                PlayerLevel.Value = playerData.level;
                Level.Value = playerData.level;
                // Update XpToNextLevel BEFORE CurrentXp to ensure correct progress calculation
                XpToNextLevel.Value = playerData.resources.xpToNextLevel;
                CurrentXp.Value = playerData.resources.currentXP;
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

        private void RecalculateCoinsPerTap()
        {
            if (_playerService == null) return;
            var baseIncome = _playerService.IncomePerTap;
            var multiplier = _playerService.IncomeMultiplier;
            var equipBonus = EquipBonusCoinsPerTap?.Value ?? 0f;
            var effective = Mathf.Max(0, Mathf.RoundToInt((baseIncome + equipBonus) * multiplier));
            CoinsPerTap.Value = effective;
        }
        
        private void OnItemEquipChanged(InventoryItem item, bool isEquipped)
        {
            RecalculateEquipBonus();
        }
        
        private void OnInventoryLoaded()
        {
            RecalculateEquipBonus();
        }
        
        private void RecalculateEquipBonus()
        {
            if (_inventoryService == null) return;
            
            var bonuses = EquippedStatsCalculator.Calculate(_inventoryService);
            var coins = EquippedStatsCalculator.GetBonus(bonuses, StatType.CoinsPerTap);
            EquipBonusCoinsPerTap.Value = coins.Flat;
            
            RecalculateCoinsPerTap();
            RecalculateProfitPerTap(bonuses);
            RecalculateProfitPerHour(bonuses);
        }
        
        private void RecalculateProfitPerTap(System.Collections.Generic.Dictionary<StatType, EquippedStatsCalculator.StatBonus> bonuses)
        {
            if (_playerService == null) return;
            
            var baseIncome = _playerService.IncomePerTap;
            var multiplier = _playerService.IncomeMultiplier;
            var coinsBonus = EquippedStatsCalculator.GetBonus(bonuses, StatType.CoinsPerTap);
            
            // base + flat equipment bonus, then multiplied
            var total = Mathf.RoundToInt((baseIncome + coinsBonus.Flat) * multiplier * (1f + coinsBonus.Percent / 100f));
            ProfitPerTap.Value = Mathf.Max(0, total);
        }
        
        private void RecalculateProfitPerHour(System.Collections.Generic.Dictionary<StatType, EquippedStatsCalculator.StatBonus> bonuses)
        {
            var baseProfitPerHour = _miningBalanceConfig != null ? _miningBalanceConfig.profitPerHour : 0;
            var offlineBonus = EquippedStatsCalculator.GetBonus(bonuses, StatType.OfflineBonusPercent);
            
            // base profit per hour + percentage boost from equipment
            var total = Mathf.RoundToInt(baseProfitPerHour * (1f + offlineBonus.Percent / 100f) + offlineBonus.Flat);
            ProfitPerHour.Value = Mathf.Max(0, total);
        }

        public override void Dispose()
        {
            // Отписываемся от событий
            if (_playerService != null)
            {
                _playerService.OnResourcesChanged -= OnPlayerResourcesChanged;
                _playerService.OnPlayerDataChanged -= OnPlayerDataChanged;
                _playerService.OnLevelUp -= OnLevelUp;
            }

            if (_tapController != null)
            {
                _tapController.OnHitsChanged -= OnHitsChanged;
                _tapController.OnIncomeMultiplierChanged -= OnIncomeMultiplierChanged;
            }
            
            if (_inventoryService != null)
            {
                _inventoryService.OnItemEquipChanged -= OnItemEquipChanged;
                _inventoryService.OnInventoryLoaded -= OnInventoryLoaded;
            }

            PlayerName?.Dispose();
            PlayerLevel?.Dispose();
            IsLoading?.Dispose();
            TotalCoins?.Dispose();
            CoinsPerTap?.Dispose();
            HitsCurrent?.Dispose();
            HitsMax?.Dispose();
            EquipBonusCoinsPerTap?.Dispose();
            ProfitPerTap?.Dispose();
            ProfitPerHour?.Dispose();
            
            base.Dispose();
        }
    }
}