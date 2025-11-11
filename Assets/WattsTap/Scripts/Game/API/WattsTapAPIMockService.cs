using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WattsTap.Game.Player;

namespace WattsTap.Game.API
{
    /// <summary>
    /// Mock-реализация API сервиса для тестирования в редакторе
    /// Симулирует серверные ответы без реального бэкенда
    /// </summary>
    public class WattsTapAPIMockService : IWattsTapAPIService
    {
        private string _authToken;
        private string _sessionId;
        private PlayerData _mockPlayerData;
        private int _tapCounter;
        private DateTime _lastLogoutTime;
        private MockAPIConfig _config;

        // Публичные поля для контроля из Editor Window
        public bool simulateNetworkDelay = true;
        public float minDelay = 0.1f;
        public float maxDelay = 0.5f;
        public float errorChance;

        public int InitializationOrder => 5;
        public bool IsInitialized { get; private set; }

        public WattsTapAPIMockService(MockAPIConfig config = null)
        {
            _config = config;
            if (_config != null)
            {
                simulateNetworkDelay = _config.simulateNetworkDelay;
                minDelay = _config.minDelay;
                maxDelay = _config.maxDelay;
                errorChance = _config.errorChance;
            }
        }

        public void Initialize()
        {
            _sessionId = Guid.NewGuid().ToString();
            
            // Устанавливаем время последнего выхода из конфига
            float offlineHours = _config != null ? _config.offlineHours : 1f;
            _lastLogoutTime = DateTime.UtcNow.AddHours(-offlineHours);
            
            InitializeMockPlayerData();
            
            IsInitialized = true;
            
            LogInfo($"Service initialized with session: {_sessionId}");
            LogInfo($"Network delay: {simulateNetworkDelay}, Error chance: {errorChance * 100}%");
        }

        public void Shutdown()
        {
            _authToken = null;
            _lastLogoutTime = DateTime.UtcNow;
            IsInitialized = false;
        }

        private void InitializeMockPlayerData()
        {
            long startingWatts = _config != null ? _config.startingWatts : 10000;
            int startingLevel = _config != null ? _config.startingLevel : 5;
            int startingHits = _config != null ? _config.startingHits : 18;
            int maxHits = _config != null ? _config.maxHits : 20;
            long incomePerTap = _config != null ? _config.incomePerTap : 50;
            long incomePerHour = _config != null ? _config.incomePerHour : 1000;

            _mockPlayerData = new PlayerData
            {
                playerId = Guid.NewGuid().ToString(),
                nickname = "MockPlayer",
                level = startingLevel,
                avatarUrl = "https://via.placeholder.com/150",
                telegramUserId = 123456789,
                tonWalletAddress = "",
                resources = new PlayerResources
                {
                    watts = startingWatts,
                    currentEnergy = 80,
                    maxEnergy = 100,
                    currentXP = 2500,
                    xpToNextLevel = CalculateXpForNextLevel(startingLevel),
                    kiloWattTokens = 0,
                    currentHits = startingHits,
                    maxHits = maxHits
                },
                stats = new PlayerStats
                {
                    totalTaps = 500,
                    totalPlayTimeSeconds = 3600,
                    incomePerHour = incomePerHour,
                    incomePerTap = incomePerTap,
                    friendsCount = 3,
                    upgradesPurchased = 5,
                    chestsOpened = 2,
                    tournamentRank = 0,
                    bestTournamentRank = 0,
                    lastLoginTime = DateTime.UtcNow
                },
                inventory = new InventoryData(),
                dailyLoginStreak = 3,
                lastDailyBonusDate = DateTime.UtcNow.AddDays(-1),
                createdAt = DateTime.UtcNow.AddDays(-30),
                updatedAt = DateTime.UtcNow
            };
        }

        // Аутентификация
        public IEnumerator Authenticate(string initData, Action<AuthResponse> onSuccess, Action<string> onError)
        {
            var startTime = Time.realtimeSinceStartup;
            yield return SimulateNetworkDelay();

            if (ShouldSimulateError())
            {
                LogError("Authentication", GetErrorMessage());
                onError?.Invoke(GetErrorMessage());
                yield break;
            }

            _authToken = $"mock_token_{Guid.NewGuid().ToString().Substring(0, 8)}";
            
            var response = new AuthResponse
            {
                token = _authToken,
                expiresIn = 86400,
                player = new PlayerInfo
                {
                    playerId = _mockPlayerData.playerId,
                    nickname = _mockPlayerData.nickname,
                    level = _mockPlayerData.level,
                    isNewPlayer = false
                }
            };

            LogSuccess("Authenticate", $"Authenticated as {response.player.nickname}", startTime);
            onSuccess?.Invoke(response);
        }

        // Отправка пакета тапов
        public IEnumerator SendTapBatch(List<TapData> taps, Action<TapBatchResponse> onSuccess, Action<string> onError)
        {
            var startTime = Time.realtimeSinceStartup;
            yield return SimulateNetworkDelay();

            if (ShouldSimulateError())
            {
                LogError("SendTapBatch", GetErrorMessage());
                onError?.Invoke(GetErrorMessage());
                yield break;
            }

            int validTaps = taps.Count;
            int invalidTaps = 0;

            // Проверяем доступность ударов (если не infinite mode)
            bool infiniteHits = _config != null && _config.infiniteHits;
            if (!infiniteHits && _mockPlayerData.resources.currentHits < validTaps)
            {
                validTaps = _mockPlayerData.resources.currentHits;
                invalidTaps = taps.Count - validTaps;
            }

            // Рассчитываем награды
            long wattsPerTap = _mockPlayerData.stats.incomePerTap;
            float xpMultiplier = _config != null ? _config.xpMultiplier : 1f;
            long xpPerTap = (long)(5 * xpMultiplier);
            long totalWatts = wattsPerTap * validTaps;
            long totalXp = xpPerTap * validTaps;

            // Обновляем ресурсы
            if (!infiniteHits)
            {
                _mockPlayerData.resources.currentHits -= validTaps;
            }
            _mockPlayerData.resources.watts += totalWatts;
            _mockPlayerData.resources.currentXP += totalXp;
            _mockPlayerData.stats.totalTaps += validTaps;
            _tapCounter += validTaps;

            // Проверяем level up
            LevelUpInfo levelUp = null;
            bool autoLevelUp = _config == null || _config.autoLevelUp;
            
            if (autoLevelUp && _mockPlayerData.resources.currentXP >= _mockPlayerData.resources.xpToNextLevel)
            {
                _mockPlayerData.level++;
                _mockPlayerData.resources.currentXP -= _mockPlayerData.resources.xpToNextLevel;
                _mockPlayerData.resources.xpToNextLevel = CalculateXpForNextLevel(_mockPlayerData.level);
                
                levelUp = new LevelUpInfo
                {
                    newLevel = _mockPlayerData.level,
                    rewards = new Rewards
                    {
                        watts = 1000,
                        xp = 0,
                        kiloWattTokens = null,
                        items = new List<InventoryItem>()
                    }
                };

                LogInfo($"🎉 LEVEL UP! New level: {_mockPlayerData.level}");
            }

            // Создаём эффекты
            var effects = new List<TapEffect>();
            for (int i = 0; i < Mathf.Min(validTaps, taps.Count); i++)
            {
                effects.Add(new TapEffect
                {
                    type = "tap_success",
                    position = taps[i].screenPosition,
                    value = wattsPerTap,
                    multiplier = 1.0f
                });
            }

            // Комбо каждые 5 тапов
            if (_tapCounter % 5 == 0 && validTaps > 0)
            {
                effects.Add(new TapEffect
                {
                    type = "combo",
                    position = taps[0].screenPosition,
                    value = 0,
                    multiplier = 1.2f
                });
            }

            var response = new TapBatchResponse
            {
                validTapsCount = validTaps,
                invalidTapsCount = invalidTaps,
                wattsEarned = totalWatts,
                xpEarned = totalXp,
                resources = CloneResources(_mockPlayerData.resources),
                effects = effects,
                levelUp = levelUp
            };

            LogSuccess("SendTapBatch", $"{validTaps} taps, +{totalWatts} watts, +{totalXp} XP", startTime);
            onSuccess?.Invoke(response);
        }

        // Получение данных игрока
        public IEnumerator GetPlayerData(Action<PlayerData> onSuccess, Action<string> onError)
        {
            var startTime = Time.realtimeSinceStartup;
            yield return SimulateNetworkDelay();

            if (ShouldSimulateError())
            {
                LogError("GetPlayerData", GetErrorMessage());
                onError?.Invoke(GetErrorMessage());
                yield break;
            }

            _mockPlayerData.updatedAt = DateTime.UtcNow;
            
            LogSuccess("GetPlayerData", $"Level {_mockPlayerData.level}, {_mockPlayerData.resources.watts} watts", startTime);
            onSuccess?.Invoke(ClonePlayerData(_mockPlayerData));
        }

        // Синхронизация состояния
        public IEnumerator SyncGameState(Action<SyncResponse> onSuccess, Action<string> onError)
        {
            var startTime = Time.realtimeSinceStartup;
            yield return SimulateNetworkDelay();

            if (ShouldSimulateError())
            {
                LogError("SyncGameState", GetErrorMessage());
                onError?.Invoke(GetErrorMessage());
                yield break;
            }

            // Восстанавливаем удары
            int hitsRecovered = Mathf.Min(
                _mockPlayerData.resources.maxHits - _mockPlayerData.resources.currentHits,
                5 // Восстанавливаем до 5 ударов
            );
            _mockPlayerData.resources.currentHits += hitsRecovered;

            var response = new SyncResponse
            {
                resources = CloneResources(_mockPlayerData.resources),
                offlineRewards = null,
                hitsRecovered = hitsRecovered
            };

            LogSuccess("SyncGameState", $"recovered {hitsRecovered} hits", startTime);
            onSuccess?.Invoke(response);
        }

        // Получение оффлайн бонуса
        public IEnumerator ClaimOfflineBonus(Action<OfflineRewards> onSuccess, Action<string> onError)
        {
            var startTime = Time.realtimeSinceStartup;
            yield return SimulateNetworkDelay();

            if (ShouldSimulateError())
            {
                LogError("ClaimOfflineBonus", GetErrorMessage());
                onError?.Invoke(GetErrorMessage());
                yield break;
            }

            var now = DateTime.UtcNow;
            var offlineDuration = (int)(now - _lastLogoutTime).TotalSeconds;
            var maxDuration = 4 * 3600; // 4 часа
            offlineDuration = Mathf.Min(offlineDuration, maxDuration);

            var incomePerHour = _mockPlayerData.stats.incomePerHour;
            var wattsEarned = (long)(incomePerHour * (offlineDuration / 3600f) * 1.5f); // 1.5x множитель

            _mockPlayerData.resources.watts += wattsEarned;
            _lastLogoutTime = now;

            var rewards = new OfflineRewards
            {
                wattsEarned = wattsEarned,
                offlineDuration = offlineDuration,
                multiplier = 1.5f,
                maxDuration = maxDuration
            };

            LogSuccess("ClaimOfflineBonus", $"+{wattsEarned} watts for {offlineDuration}s offline", startTime);
            onSuccess?.Invoke(rewards);
        }

        // Вспомогательные методы
        private IEnumerator SimulateNetworkDelay()
        {
            if (!simulateNetworkDelay)
                yield break;

            float delay = UnityEngine.Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);
        }

        private bool ShouldSimulateError()
        {
            return UnityEngine.Random.value < errorChance;
        }

        private string GetErrorMessage()
        {
            if (_config != null)
                return _config.GetRandomErrorMessage();
            
            return "Mock error: Request failed";
        }

        private long CalculateXpForNextLevel(int level)
        {
            return 1000 * level + 500 * (level - 1);
        }

        private PlayerData ClonePlayerData(PlayerData original)
        {
            // Простое клонирование через JSON (для мока достаточно)
            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<PlayerData>(json);
        }

        private PlayerResources CloneResources(PlayerResources original)
        {
            return new PlayerResources
            {
                watts = original.watts,
                currentEnergy = original.currentEnergy,
                maxEnergy = original.maxEnergy,
                currentXP = original.currentXP,
                xpToNextLevel = original.xpToNextLevel,
                kiloWattTokens = original.kiloWattTokens,
                currentHits = original.currentHits,
                maxHits = original.maxHits
            };
        }

        // Публичные методы для управления моком из редактора
        public void ResetPlayerData()
        {
            InitializeMockPlayerData();
            Debug.Log("[MockAPI] Player data reset");
        }

        public void AddWatts(long amount)
        {
            _mockPlayerData.resources.watts += amount;
            Debug.Log($"[MockAPI] Added {amount} watts. New balance: {_mockPlayerData.resources.watts}");
        }

        public void AddXp(long amount)
        {
            _mockPlayerData.resources.currentXP += amount;
            Debug.Log($"[MockAPI] Added {amount} XP. Current XP: {_mockPlayerData.resources.currentXP}");
        }

        public void SetLevel(int level)
        {
            _mockPlayerData.level = level;
            _mockPlayerData.resources.xpToNextLevel = CalculateXpForNextLevel(level);
            Debug.Log($"[MockAPI] Set level to {level}");
        }

        public void RestoreHits()
        {
            _mockPlayerData.resources.currentHits = _mockPlayerData.resources.maxHits;
            Debug.Log($"[MockAPI] Hits restored to {_mockPlayerData.resources.maxHits}");
        }

        public void SetErrorChance(float chance)
        {
            errorChance = Mathf.Clamp01(chance);
            Debug.Log($"[MockAPI] Error chance set to {errorChance * 100}%");
        }

        public PlayerData GetCurrentMockData()
        {
            return _mockPlayerData;
        }

        // Logging helpers
        private void LogInfo(string message)
        {
            if (_config == null || _config.verboseLogging)
            {
                Debug.Log($"[MockAPI] {message}");
            }
        }

        private void LogSuccess(string method, string message, float startTime)
        {
            if (_config == null || _config.verboseLogging)
            {
                string timeInfo = "";
                if (_config != null && _config.logResponseTime)
                {
                    float duration = (Time.realtimeSinceStartup - startTime) * 1000f;
                    timeInfo = $" ({duration:F0}ms)";
                }
                Debug.Log($"[MockAPI] ✓ {method}: {message}{timeInfo}");
            }
        }

        private void LogError(string method, string error)
        {
            Debug.LogError($"[MockAPI] ✗ {method}: {error}");
        }
    }
}
