using System;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.Configs;
using WattsTap.Core.DataPersistence;
using WattsTap.Scripts.Game.GlobalConfigs;

namespace WattsTap.Scripts.Game.Mining
{
    /// <summary>
    /// Сервис управления балансом майнинга с лимитами по дням
    /// </summary>
    public class MiningBalanceService : IMiningBalanceService
    {
        private const string ProgressDataKey = "MiningProgressData";
        
        private MiningProgressData _progressData;
        private MiningBalanceConfig _miningConfig;
        private CriticalHitConfig _critConfig;
        private IDataPersistenceService _dataPersistence;
        
        public int InitializationOrder => 15; // После PlayerService (10) и DataPersistence (5)
        public bool IsInitialized { get; private set; }
        
        public int CurrentDay => _progressData?.currentDay ?? 1;
        public MiningProgressData ProgressData => _progressData;
        
        public event Action<int> OnDayChanged;
        public event Action<MiningProgressData> OnProgressChanged;
        public event Action<string> OnDayLimitReached;

        public void Initialize()
        {
            if (IsInitialized) return;
            
            // Получаем сервисы
            var configService = ServiceLocator.Get<IConfigService>();
            _dataPersistence = ServiceLocator.Get<IDataPersistenceService>();
            
            // Загружаем конфигурации
            _miningConfig = configService.GetConfig<MiningBalanceConfig>("MiningBalanceConfig");
            _critConfig = configService.GetConfig<CriticalHitConfig>("CriticalHitConfig");
            
            if (_miningConfig == null)
            {
                Debug.LogError("[MiningBalanceService] MiningBalanceConfig not found! Service will not work properly.");
                _miningConfig = ScriptableObject.CreateInstance<MiningBalanceConfig>();
            }
            
            // Загружаем данные прогресса
            LoadProgress();
            
            // Проверяем, не нужно ли сбросить дневной прогресс
            CheckDayReset();
            
            IsInitialized = true;
            Debug.Log($"<color=green>[MiningBalanceService] Initialized. Current day: {CurrentDay}</color>");
        }

        public void Shutdown()
        {
            SaveProgress();
            IsInitialized = false;
            Debug.Log("[MiningBalanceService] Shutdown");
        }

        public void SetDay(int day)
        {
            if (day < 1 || day > 30)
            {
                Debug.LogWarning($"[MiningBalanceService] Day {day} is out of range (1-30)");
                return;
            }
            
            int previousDay = _progressData.currentDay;
            _progressData.currentDay = day;
            _progressData.updatedAt = DateTime.UtcNow;
            
            // Сбрасываем дневной прогресс
            ResetDailyProgress();
            
            Debug.Log($"[MiningBalanceService] Day changed: {previousDay} → {day}");
            OnDayChanged?.Invoke(day);
            OnProgressChanged?.Invoke(_progressData);
            SaveProgress();
        }

        public bool ProcessTap(out long coins, out long exp)
        {
            coins = 0;
            exp = 0;
            
            if (!CanTap())
            {
                Debug.LogWarning("[MiningBalanceService] Cannot tap - daily limit reached");
                OnDayLimitReached?.Invoke("Daily tap limit reached");
                return false;
            }
            
            // Базовые значения из конфига
            long baseCoins = _miningConfig.coinsPerTap;
            long baseExp = _miningConfig.expPerTap;
            
            // Проверяем критический удар
            bool isCritical = false;
            if (_critConfig != null && _critConfig.RollCriticalHit())
            {
                baseCoins = _critConfig.CalculateCriticalDamage(baseCoins);
                isCritical = true;
            }
            
            coins = baseCoins;
            exp = baseExp;
            
            // Обновляем прогресс
            _progressData.totalTaps++;
            _progressData.tapsToday++;
            _progressData.totalCoinsEarned += coins;
            _progressData.totalExpEarned += exp;
            _progressData.coinsEarnedToday += coins;
            _progressData.expEarnedToday += exp;
            _progressData.lastTapTime = DateTime.UtcNow;
            _progressData.updatedAt = DateTime.UtcNow;
            
            if (isCritical)
            {
                Debug.Log($"<color=yellow>[MiningBalanceService] CRITICAL TAP!</color> Coins: {coins}, Exp: {exp}");
            }
            
            OnProgressChanged?.Invoke(_progressData);
            
            // Автосохранение каждые 10 тапов
            if (_progressData.totalTaps % 10 == 0)
            {
                SaveProgress();
            }
            
            return true;
        }

        public bool CanTap()
        {
            if (_progressData == null) return false;
            
            var dayProgression = GetCurrentDayProgression();
            if (dayProgression == null) return true; // Нет лимитов, если конфиг не найден
            
            // Проверяем лимит тапов за день
            return _progressData.tapsToday < dayProgression.tapsPerDay;
        }

        public int GetRemainingTapsToday()
        {
            var dayProgression = GetCurrentDayProgression();
            if (dayProgression == null) return int.MaxValue;
            
            int remaining = dayProgression.tapsPerDay - _progressData.tapsToday;
            return Math.Max(0, remaining);
        }

        public DailyProgressionData GetCurrentDayProgression()
        {
            if (_miningConfig == null) return null;
            return _miningConfig.GetDayProgression(CurrentDay);
        }

        public void ResetProgress()
        {
            Debug.Log("[MiningBalanceService] Resetting all progress data");
            _progressData = new MiningProgressData();
            _dataPersistence?.DeleteData(ProgressDataKey);
            OnProgressChanged?.Invoke(_progressData);
            SaveProgress();
        }

        public void SaveProgress()
        {
            if (_progressData == null || _dataPersistence == null) return;
            
            _progressData.updatedAt = DateTime.UtcNow;
            _dataPersistence.SaveData(ProgressDataKey, _progressData);
            Debug.Log($"[MiningBalanceService] Progress saved. Day: {CurrentDay}, Total taps: {_progressData.totalTaps}");
        }

        public void LoadProgress()
        {
            if (_dataPersistence == null)
            {
                Debug.LogWarning("[MiningBalanceService] DataPersistence service not available, creating new progress");
                _progressData = new MiningProgressData();
                return;
            }
            
            _progressData = _dataPersistence.LoadData<MiningProgressData>(ProgressDataKey);
            
            if (_progressData == null)
            {
                Debug.Log("[MiningBalanceService] No saved progress found, creating new progress");
                _progressData = new MiningProgressData();
            }
            else
            {
                Debug.Log($"[MiningBalanceService] Progress loaded. Day: {CurrentDay}, Total taps: {_progressData.totalTaps}");
            }
            
            OnProgressChanged?.Invoke(_progressData);
        }

        /// <summary>
        /// Проверяет, нужно ли сбросить дневной прогресс (прошло 24 часа)
        /// </summary>
        private void CheckDayReset()
        {
            if (_progressData == null) return;
            
            var timeSinceReset = DateTime.UtcNow - _progressData.lastDayResetTime;
            
            // Если прошло больше 24 часов, сбрасываем дневной прогресс
            if (timeSinceReset.TotalHours >= 24)
            {
                Debug.Log($"[MiningBalanceService] 24 hours passed, resetting daily progress. Time since reset: {timeSinceReset.TotalHours:F1} hours");
                ResetDailyProgress();
                SaveProgress();
            }
        }

        /// <summary>
        /// Сбрасывает дневные счетчики (не влияет на общий прогресс)
        /// </summary>
        private void ResetDailyProgress()
        {
            _progressData.tapsToday = 0;
            _progressData.coinsEarnedToday = 0;
            _progressData.expEarnedToday = 0;
            _progressData.lastDayResetTime = DateTime.UtcNow;
            _progressData.updatedAt = DateTime.UtcNow;
            
            Debug.Log("[MiningBalanceService] Daily progress reset");
        }
    }
}
