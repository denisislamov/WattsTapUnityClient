using System;
using System.Linq;
using UnityEngine;
using WattsTap.Core;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Сервис для управления данными игрока
    /// </summary>
    public class PlayerService : IPlayerService
    {
        private const string PlayerDataKey = "PlayerData";
        
        private PlayerData _playerData;
        private IResourceManager _resourceManager;
        private ResourceConfig _resourceConfig;
        private float _energyRestoreTimer;
        
        public int InitializationOrder => 10;
        public bool IsInitialized { get; private set; }
        
        public event Action<PlayerData> OnPlayerDataChanged;
        public event Action<PlayerResources> OnResourcesChanged;
        public event Action<int> OnLevelUp;
        public event Action<int, int> OnEnergyChanged;
        
        /// <summary>
        /// Получить менеджер р��сурсов для прямого доступа
        /// </summary>
        public IResourceManager ResourceManager => _resourceManager;

        public void Initialize()
        {
            if (IsInitialized) return;
            
            // Load or create default config
            var configService = ServiceLocator.Get<Core.Configs.IConfigService>();
            _resourceConfig = configService.GetConfig<ResourceConfig>( "ResourceConfig");
            if (_resourceConfig == null)
            {
                Debug.LogWarning("[PlayerService] ResourceConfig not found, using defaults");
                _resourceConfig = ScriptableObject.CreateInstance<ResourceConfig>();
            }
            
            LoadPlayerData();
            
            // Initialize ResourceManager after player data is loaded
            _resourceManager = new ResourceManager(_playerData.resources);
            _resourceManager.OnResourceChanged += OnResourceManagerChanged;
            _resourceManager.OnResourceTransaction += OnResourceManagerTransaction;
            
            Application.targetFrameRate = 60;
            IsInitialized = true;
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
            // Forward resource change events
            if (type == ResourceType.Energy)
            {
                OnEnergyChanged?.Invoke((int)newValue, _playerData.resources.maxEnergy);
            }
            OnResourcesChanged?.Invoke(_playerData.resources);
            _playerData.updatedAt = DateTime.UtcNow;
        }
        
        private void OnResourceManagerTransaction(ResourceTransaction transaction)
        {
            Debug.Log($"[PlayerService] Resource transaction: {transaction.ResourceType} {(transaction.Amount >= 0 ? "+" : "")}{transaction.Amount} ({transaction.PreviousValue} -> {transaction.NewValue})");
        }

        public PlayerData GetPlayerData() => _playerData;

        public void LoadPlayerData()
        {
            var json = PlayerPrefs.GetString(PlayerDataKey, string.Empty);
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("[PlayerService] No saved data found, creating new player");
                _playerData = new PlayerData();
            }
            else
            {
                try
                {
                    _playerData = JsonUtility.FromJson<PlayerData>(json);
                    var offlineIncome = CalculateOfflineIncome();
                    if (offlineIncome > 0)
                    {
                        AddWatts(offlineIncome);
                        Debug.Log($"[PlayerService] Offline income: {offlineIncome} Watts");
                    }
                    _playerData.stats.lastLoginTime = DateTime.UtcNow;
                    Debug.Log($"[PlayerService] Loaded player data: {_playerData.nickname}, Level {_playerData.level}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[PlayerService] Failed to load player data: {e.Message}");
                    _playerData = new PlayerData();
                }
            }
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        public void SavePlayerData()
        {
            if (_playerData == null) return;
            _playerData.updatedAt = DateTime.UtcNow;
            _playerData.stats.lastLogoutTime = DateTime.UtcNow;
            var json = JsonUtility.ToJson(_playerData, true);
            PlayerPrefs.SetString(PlayerDataKey, json);
            PlayerPrefs.Save();
            Debug.Log("[PlayerService] Player data saved");
        }

        public void CreateNewPlayer(string nickname, long telegramUserId)
        {
            _playerData = new PlayerData
            {
                nickname = nickname,
                telegramUserId = telegramUserId,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow
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
            _playerData.resources.xpToNextLevel = _resourceConfig.CalculateXpForLevel(_playerData.level);
            
            var wattsReward = _resourceConfig.CalculateLevelUpReward(_playerData.level);
            _resourceManager.AddResource(ResourceType.Watts, wattsReward, false);
            
            if (_resourceConfig.restoreEnergyOnLevelUp)
            {
                _resourceManager.SetResource(ResourceType.Energy, _playerData.resources.maxEnergy);
            }
            
            Debug.Log($"[PlayerService] Level UP! New level: {_playerData.level}");
            OnLevelUp?.Invoke(_playerData.level);
            OnPlayerDataChanged?.Invoke(_playerData);
            OnResourcesChanged?.Invoke(_playerData.resources);
        }

        public bool UseEnergy(int amount = 1)
        {
            var transaction = _resourceManager.SpendResource(ResourceType.Energy, amount);
            return transaction.Success;
        }

        public void RestoreEnergy(int amount)
        {
            var currentEnergy = _resourceManager.GetResource(ResourceType.Energy);
            var maxEnergy = _resourceManager.GetMaxResource(ResourceType.Energy);
            var newEnergy = Math.Min(currentEnergy + amount, maxEnergy);
            _resourceManager.SetResource(ResourceType.Energy, newEnergy, true);
        }

        public bool PerformTap()
        {
            var energyCost = _resourceConfig.energyCostPerTap;
            if (!_resourceManager.HasEnough(ResourceType.Energy, energyCost)) 
                return false;
            
            _resourceManager.SpendResource(ResourceType.Energy, energyCost);
            
            var tapIncome = _playerData.stats.incomePerTap + CalculateTotalEquipmentBonus();
            _resourceManager.AddResource(ResourceType.Watts, tapIncome);
            _resourceManager.AddResource(ResourceType.Experience, _resourceConfig.xpPerTap);
            
            _playerData.stats.totalTaps++;
            
            // Check for level up after adding experience
            while (_playerData.resources.currentXP >= _playerData.resources.xpToNextLevel)
            {
                LevelUp();
            }
            
            return true;
        }

        private long CalculateTotalEquipmentBonus()
        {
            return _playerData.inventory.items.Where(IsItemEquipped).Sum(item => item.tapIncomeBonus);
        }

        private bool IsItemEquipped(InventoryItem item)
        {
            return item.type switch
            {
                ItemType.Weapon => item.itemId == _playerData.inventory.equippedWeaponId,
                ItemType.Helmet => item.itemId == _playerData.inventory.equippedHelmetId,
                ItemType.Armor => item.itemId == _playerData.inventory.equippedArmorId,
                ItemType.Boots => item.itemId == _playerData.inventory.equippedBootsId,
                _ => false
            };
        }

        public void AddItem(InventoryItem item)
        {
            if (item == null) return;
            _playerData.inventory.items.Add(item);
            OnPlayerDataChanged?.Invoke(_playerData);
            Debug.Log($"[PlayerService] Added item: {item.type} ({item.rarity})");
        }

        public bool EquipItem(string itemId)
        {
            var item = _playerData.inventory.items.FirstOrDefault(i => i.itemId == itemId);
            if (item == null) return false;
            
            switch (item.type)
            {
                case ItemType.Weapon:
                    _playerData.inventory.equippedWeaponId = itemId;
                    break;
                case ItemType.Helmet:
                    _playerData.inventory.equippedHelmetId = itemId;
                    break;
                case ItemType.Armor:
                    _playerData.inventory.equippedArmorId = itemId;
                    break;
                case ItemType.Boots:
                    _playerData.inventory.equippedBootsId = itemId;
                    break;
            }
            
            RecalculateMaxEnergy();
            OnPlayerDataChanged?.Invoke(_playerData);
            return true;
        }

        public void UnequipItem(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Weapon:
                    _playerData.inventory.equippedWeaponId = string.Empty;
                    break;
                case ItemType.Helmet:
                    _playerData.inventory.equippedHelmetId = string.Empty;
                    break;
                case ItemType.Armor:
                    _playerData.inventory.equippedArmorId = string.Empty;
                    break;
                case ItemType.Boots:
                    _playerData.inventory.equippedBootsId = string.Empty;
                    break;
            }
            RecalculateMaxEnergy();
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        private void RecalculateMaxEnergy()
        {
            int baseEnergy = _resourceConfig.baseMaxEnergy;
            int bonus = _playerData.inventory.items.Where(IsItemEquipped).Sum(item => item.energyBonus);
            var newMaxEnergy = baseEnergy + bonus;
            _resourceManager.SetMaxResource(ResourceType.Energy, newMaxEnergy);
        }

        public bool UpgradeItem(string itemId, long cost)
        {
            var item = _playerData.inventory.items.FirstOrDefault(i => i.itemId == itemId);
            if (item == null) return false;
            
            var transaction = _resourceManager.SpendResource(ResourceType.Watts, cost);
            if (!transaction.Success) return false;
            
            item.level++;
            item.tapIncomeBonus = (long)(item.tapIncomeBonus * 1.2);
            item.passiveIncomeBonus = (long)(item.passiveIncomeBonus * 1.2);
            item.energyBonus = (int)(item.energyBonus * 1.1);
            _playerData.stats.upgradesPurchased++;
            RecalculateMaxEnergy();
            OnPlayerDataChanged?.Invoke(_playerData);
            return true;
        }

        public void SellItem(string itemId)
        {
            var item = _playerData.inventory.items.FirstOrDefault(i => i.itemId == itemId);
            if (item == null) return;
            
            if (IsItemEquipped(item))
            {
                UnequipItem(item.type);
            }
            
            long sellPrice = CalculateItemSellPrice(item);
            _resourceManager.AddResource(ResourceType.Watts, sellPrice);
            _playerData.inventory.items.Remove(item);
            OnPlayerDataChanged?.Invoke(_playerData);
            Debug.Log($"[PlayerService] Sold item for {sellPrice} Watts");
        }

        private long CalculateItemSellPrice(InventoryItem item)
        {
            int rarityMultiplier = item.rarity switch
            {
                ItemRarity.Common => 10,
                ItemRarity.Uncommon => 25,
                ItemRarity.Rare => 50,
                ItemRarity.Epic => 100,
                ItemRarity.Legendary => 250,
                _ => 10
            };
            return rarityMultiplier * item.level;
        }

        public void ConnectWallet(string walletAddress)
        {
            if (string.IsNullOrEmpty(walletAddress)) return;
            _playerData.tonWalletAddress = walletAddress;
            SavePlayerData();
            OnPlayerDataChanged?.Invoke(_playerData);
            Debug.Log($"[PlayerService] Wallet connected: {walletAddress}");
        }

        public long CalculateOfflineIncome()
        {
            var lastLogout = _playerData.stats.lastLogoutTime;
            var now = DateTime.UtcNow;
            var diff = now - lastLogout;
            
            var maxHours = _resourceConfig.maxOfflineIncomeHours;
            var hours = Math.Min(diff.TotalHours, maxHours);
            
            var offlineIncome = (long)(hours * _playerData.stats.incomePerHour * _resourceConfig.offlineIncomeMultiplier);
            return offlineIncome;
        }

        public void UpdateTournamentRank(int rank)
        {
            _playerData.stats.tournamentRank = rank;
            if (rank > 0 && (rank < _playerData.stats.bestTournamentRank || _playerData.stats.bestTournamentRank == 0))
            {
                _playerData.stats.bestTournamentRank = rank;
            }
            SavePlayerData();
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        public void InviteFriend()
        {
            _playerData.stats.friendsCount++;
            SavePlayerData();
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        public bool ClaimDailyBonus(out int streakDay)
        {
            var now = DateTime.UtcNow;
            var lastBonus = _playerData.lastDailyBonusDate;
            var daysSinceLastBonus = (now - lastBonus).Days;
            
            if (daysSinceLastBonus < 1)
            {
                streakDay = _playerData.dailyLoginStreak;
                return false; // Already claimed today
            }
            
            // Update streak
            if (daysSinceLastBonus == 1)
            {
                _playerData.dailyLoginStreak++;
            }
            else
            {
                _playerData.dailyLoginStreak = 1; // Reset streak
            }
            
            _playerData.lastDailyBonusDate = now;
            streakDay = _playerData.dailyLoginStreak;
            
            // Calculate reward based on streak
            var wattsReward = streakDay * 100;
            _resourceManager.AddResource(ResourceType.Watts, wattsReward);
            
            SavePlayerData();
            OnPlayerDataChanged?.Invoke(_playerData);
            Debug.Log($"[PlayerService] Daily bonus claimed: {wattsReward} Watts (Day {streakDay})");
            return true;
        }
    }
}
