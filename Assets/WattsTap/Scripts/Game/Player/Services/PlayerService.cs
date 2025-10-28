using System;
using System.Linq;
using UnityEngine;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Сервис для управления данными игрока
    /// </summary>
    public class PlayerService : IPlayerService
    {
        private const string PlayerDataKey = "PlayerData";
        private const int OfflineIncomeMaxHours = 4;
        private const float EnergyRestoreInterval = 1f;
        
        private PlayerData _playerData;
        private float _energyRestoreTimer;
        
        public int InitializationOrder => 10;
        public bool IsInitialized { get; private set; }
        
        public event Action<PlayerData> OnPlayerDataChanged;
        public event Action<PlayerResources> OnResourcesChanged;
        public event Action<int> OnLevelUp;
        public event Action<int, int> OnEnergyChanged;

        public void Initialize()
        {
            if (IsInitialized) return;
            LoadPlayerData();
            Application.targetFrameRate = 60;
            IsInitialized = true;
            Debug.Log("<color=green>[PlayerService] Initialized</color>");
        }

        public void Shutdown()
        {
            SavePlayerData();
            IsInitialized = false;
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
            _playerData.resources.watts += amount;
            _playerData.updatedAt = DateTime.UtcNow;
            OnResourcesChanged?.Invoke(_playerData.resources);
        }

        public bool SpendWatts(long amount)
        {
            if (amount <= 0 || _playerData.resources.watts < amount) return false;
            _playerData.resources.watts -= amount;
            _playerData.updatedAt = DateTime.UtcNow;
            OnResourcesChanged?.Invoke(_playerData.resources);
            return true;
        }

        public void AddExperience(long amount)
        {
            if (amount <= 0) return;
            _playerData.resources.currentXP += amount;
            while (_playerData.resources.currentXP >= _playerData.resources.xpToNextLevel)
            {
                LevelUp();
            }
            OnResourcesChanged?.Invoke(_playerData.resources);
        }

        private void LevelUp()
        {
            _playerData.resources.currentXP -= _playerData.resources.xpToNextLevel;
            _playerData.level++;
            _playerData.resources.xpToNextLevel = CalculateXpForNextLevel(_playerData.level);
            var wattsReward = _playerData.level * 100;
            AddWatts(wattsReward);
            _playerData.resources.currentEnergy = _playerData.resources.maxEnergy;
            Debug.Log($"[PlayerService] Level UP! New level: {_playerData.level}");
            OnLevelUp?.Invoke(_playerData.level);
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        private long CalculateXpForNextLevel(int level)
        {
            return (long)(100 * Math.Pow(level, 1.5));
        }

        public bool UseEnergy(int amount = 1)
        {
            if (_playerData.resources.currentEnergy < amount) return false;
            _playerData.resources.currentEnergy -= amount;
            OnEnergyChanged?.Invoke(_playerData.resources.currentEnergy, _playerData.resources.maxEnergy);
            return true;
        }

        public void RestoreEnergy(int amount)
        {
            _playerData.resources.currentEnergy = Mathf.Min(_playerData.resources.currentEnergy + amount, _playerData.resources.maxEnergy);
            OnEnergyChanged?.Invoke(_playerData.resources.currentEnergy, _playerData.resources.maxEnergy);
        }

        public bool PerformTap()
        {
            if (!UseEnergy()) return false;
            var tapIncome = _playerData.stats.incomePerTap + CalculateTotalEquipmentBonus();
            AddWatts(tapIncome);
            AddExperience(1);
            _playerData.stats.totalTaps++;
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
            int baseEnergy = 100;
            int bonus = _playerData.inventory.items.Where(IsItemEquipped).Sum(item => item.energyBonus);
            _playerData.resources.maxEnergy = baseEnergy + bonus;
            OnEnergyChanged?.Invoke(_playerData.resources.currentEnergy, _playerData.resources.maxEnergy);
        }

        public bool UpgradeItem(string itemId, long cost)
        {
            var item = _playerData.inventory.items.FirstOrDefault(i => i.itemId == itemId);
            if (item == null || !SpendWatts(cost)) return false;
            
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
            AddWatts(sellPrice);
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
            Debug.Log($"[PlayerService] Connected wallet: {walletAddress}");
        }

        public long CalculateOfflineIncome()
        {
            var timeDiff = DateTime.UtcNow - _playerData.stats.lastLogoutTime;
            var hours = Math.Min(timeDiff.TotalHours, OfflineIncomeMaxHours);
            if (hours < 0.1) return 0;
            return (long)(hours * _playerData.stats.incomePerHour);
        }

        public void UpdateTournamentRank(int rank)
        {
            _playerData.stats.tournamentRank = rank;
            if (rank < _playerData.stats.bestTournamentRank || _playerData.stats.bestTournamentRank == 0)
            {
                _playerData.stats.bestTournamentRank = rank;
            }
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        public void InviteFriend()
        {
            _playerData.stats.friendsCount++;
            var reward = 1000;
            AddWatts(reward);
            Debug.Log($"[PlayerService] Friend invited! Reward: {reward} Watts");
            OnPlayerDataChanged?.Invoke(_playerData);
        }

        public bool ClaimDailyBonus(out int streakDay)
        {
            var today = DateTime.UtcNow.Date;
            var lastBonus = _playerData.lastDailyBonusDate.Date;
            
            if (lastBonus == today)
            {
                streakDay = _playerData.dailyLoginStreak;
                return false;
            }
            
            var yesterday = today.AddDays(-1);
            if (lastBonus == yesterday)
            {
                _playerData.dailyLoginStreak++;
            }
            else
            {
                _playerData.dailyLoginStreak = 1;
            }
            
            if (_playerData.dailyLoginStreak > 7)
            {
                _playerData.dailyLoginStreak = 1;
            }
            
            _playerData.lastDailyBonusDate = DateTime.UtcNow;
            streakDay = _playerData.dailyLoginStreak;
            var reward = streakDay * 500;
            AddWatts(reward);
            Debug.Log($"[PlayerService] Daily bonus claimed! Day {streakDay}, Reward: {reward} Watts");
            SavePlayerData();
            return true;
        }

        public void UpdateEnergyRestore(float deltaTime)
        {
            if (_playerData.resources.currentEnergy >= _playerData.resources.maxEnergy)
            {
                _energyRestoreTimer = 0;
                return;
            }
            
            _energyRestoreTimer += deltaTime;
            if (_energyRestoreTimer >= EnergyRestoreInterval)
            {
                _energyRestoreTimer = 0;
                RestoreEnergy(1);
            }
        }
    }
}

