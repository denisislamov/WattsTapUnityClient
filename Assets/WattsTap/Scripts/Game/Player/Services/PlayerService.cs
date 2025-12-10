using System;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.Telegram;
using WattsTap.Core.UI;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Game.Player
{
    public class PlayerService : IPlayerService
    {
        private PlayerData _playerData;
        private IResourceManager _resourceManager;
        private IHapticFeedbackService _hapticService;
        private PlayerLevelConfig _levelConfig;
        private MiningBalanceConfig _miningBalanceConfig;
        private float _energyRestoreTimer;
        
        public int InitializationOrder => 10;
        public bool IsInitialized { get; private set; }
        
        public event Action<PlayerData> OnPlayerDataChanged;
        public event Action<PlayerResources> OnResourcesChanged;
        public event Action<int> OnLevelUp;
        public event Action<int, int> OnEnergyChanged;

        public int IncomePerTap => _miningBalanceConfig.coinsPerTap;
        public int ExperiencePerTap => _miningBalanceConfig.expPerTap;
        public int EnergyCostPerTap => _miningBalanceConfig.energyCostPerTap;
        public float IncomeMultiplier => 1f;
        public IResourceManager ResourceManager => _resourceManager;

        public void Initialize()
        {
            if (IsInitialized) return;
            
            var configService = ServiceLocator.Get<Core.Configs.IConfigService>();
            
            _levelConfig = configService.GetConfig<PlayerLevelConfig>("PlayerLevelConfig");
            _miningBalanceConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
            
            ServiceLocator.TryGet(out _hapticService);
            
            LoadPlayerData();
            
            _resourceManager = new ResourceManager(_playerData.resources);
            _resourceManager.OnResourceChanged += OnResourceManagerChanged;
            _resourceManager.OnResourceTransaction += OnResourceManagerTransaction;
            
            _playerData.resources.xpToNextLevel = _levelConfig.GetExpRequiredForLevel(_playerData.level + 1);
            
            Application.targetFrameRate = 60;
            IsInitialized = true;
            
            OnPlayerDataChanged?.Invoke(_playerData);
            
            Debug.Log("<color=green>[PlayerService] Initialized</color>");
        }

        public void Shutdown()
        {
            if (_resourceManager != null)
            {
                _resourceManager.OnResourceChanged -= OnResourceManagerChanged;
                _resourceManager.OnResourceTransaction -= OnResourceManagerTransaction;
            }
            SavePlayerData();
            IsInitialized = false;
        }
        
        private void OnResourceManagerChanged(ResourceType type, long previousValue, long newValue)
        {
            OnResourcesChanged?.Invoke(_playerData.resources);
        }
        
        private void OnResourceManagerTransaction(ResourceTransaction transaction)
        {
            Debug.Log($"[PlayerService] Resource transaction: {transaction.ResourceType} {(transaction.Amount >= 0 ? "+" : "")}{transaction.Amount} ({transaction.PreviousValue} -> {transaction.NewValue})");
        }

        public PlayerData GetPlayerData() => _playerData;

        // TODO - implement saving/loading
        public void LoadPlayerData()
        {
            Debug.Log("[PlayerService] No saved data found, creating new player");
            _playerData = new PlayerData();
            
            // TODO - load
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        /// TODO - implement saving/loading
        public void SavePlayerData()
        {
            if (_playerData == null) return;
            
            // TODO - save
            Debug.Log("[PlayerService] Player data saved");
        }

        public void CreateNewPlayer(string nickname, long telegramUserId)
        {
            _playerData = new PlayerData
            {
                nickname = nickname,
                telegramUserId = telegramUserId
            };
            SavePlayerData();
            OnPlayerDataChanged?.Invoke(_playerData);
            Debug.Log($"[PlayerService] Created new player: {nickname}");
        }

        public void AddWatts(long amount)
        {
            if (amount <= 0) return;
            _resourceManager.AddResource(ResourceType.Watts, amount);
        }

        public bool SpendWatts(long amount)
        {
            if (amount <= 0) return false;
            var transaction = _resourceManager.SpendResource(ResourceType.Watts, amount);
            return transaction.Success;
        }

        public void AddExperience(long amount)
        {
            if (amount <= 0) return;
            _resourceManager.AddResource(ResourceType.Experience, amount);
            
            // Check for level up
            while (_playerData.resources.currentXP >= _playerData.resources.xpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            _playerData.resources.currentXP -= _playerData.resources.xpToNextLevel;
            _playerData.level++;
            _playerData.resources.xpToNextLevel = _levelConfig.GetExpRequiredForLevel(_playerData.level + 1);

            var wattsReward = _levelConfig.GetRewardsForLevel(_playerData.level).coins;
            _resourceManager.AddResource(ResourceType.Watts, wattsReward, false);
            
            // Trigger haptic feedback for level up
            _hapticService?.LevelUp();
            
            Debug.Log($"[PlayerService] Level UP! New level: {_playerData.level}");
            OnLevelUp?.Invoke(_playerData.level);
            OnPlayerDataChanged?.Invoke(_playerData);
            OnResourcesChanged?.Invoke(_playerData.resources);
            ShowLevelUpPopup();
        }
        

        public bool PerformTap()
        {
             var energyCost = EnergyCostPerTap;
             if (!_resourceManager.HasEnough(ResourceType.Hits, energyCost))
             {
                 return false;
             }
             
             var tapIncome = IncomePerTap;
             _resourceManager.AddResource(ResourceType.Watts, tapIncome);
             _resourceManager.AddResource(ResourceType.Experience, _miningBalanceConfig.expPerTap);
            
             while (_playerData.resources.currentXP >= _playerData.resources.xpToNextLevel)
             {
                 LevelUp();
             }
            
             return true;
        }

        private void ShowLevelUpPopup()
        {
            if (!ServiceLocator.TryGet<IUIService>(out var uiService))
            {
                return;
            }

            if (uiService.GetViews(UIConstants.LevelUpPopUp).Count > 0)
            {
                uiService.Close(UIConstants.LevelUpPopUp);
            }

            uiService.Open(UIConstants.LevelUpPopUp);
        }
    }
}
