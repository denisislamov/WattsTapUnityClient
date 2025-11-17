using System;
using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Scripts.Game.GlobalConfigs
{
    /// <summary>
    /// Конфигурация баланса майнинга из WattsBalanceMining.csv
    /// Содержит стартовые параметры и прогрессию по дням
    /// </summary>
    [CreateAssetMenu(fileName = "MiningBalanceConfig", menuName = "WattsTap/Configs/Mining Balance Config")]
    public class MiningBalanceConfig : BaseConfig
    {
        [Header("Start Parameters")]
        [Tooltip("Монет за один тап")]
        public int coinsPerTap = 1;
        
        [Tooltip("Опыт за один тап")]
        public int expPerTap = 1;
        
        [Tooltip("Стоимость энергии за один тап")]
        public int energyCostPerTap;
        
        [Tooltip("Начальная вместимость ударов")]
        public int startCapacityHits = 1500;
        
        [Tooltip("Время восстановления 1 удара в секундах")]
        public float cooldownPerHitSec = 2f;
        
        [Tooltip("Множитель критического урона")]
        public float critMultiplier = 1.2f;
        
        [Tooltip("Шанс критического удара в процентах")]
        public float chanceCritPercent = 1f;
        
        [Tooltip("Среднее время игры в минутах")]
        public int avgPlaytimeMinutes = 15;
        
        [Tooltip("Тапов в секунду")]
        public int tapsPerSecond = 10;
        
        [Tooltip("Профит в час (пассивный доход)")]
        public int profitPerHour = 500;
        
        [Tooltip("Максимум часов оффлайн дохода")]
        public int maxHoursOffline = 3;
        
        [Tooltip("Сессий в день")]
        public int sessionsPerDay = 2;
        
        [Header("Daily Progression")]
        [Tooltip("Данные прогрессии по дням")]
        public DailyProgressionData[] dailyProgression = new DailyProgressionData[30];
        
        /// <summary>
        /// Получить данные прогрессии для конкретного дня
        /// </summary>
        public DailyProgressionData GetDayProgression(int day)
        {
            if (day < 1 || day > dailyProgression.Length)
            {
                Debug.LogWarning($"[MiningBalanceConfig] Day {day} out of range. Returning default values.");
                return new DailyProgressionData();
            }
            
            return dailyProgression[day - 1];
        }
        
        /// <summary>
        /// Рассчитать ожидаемый доход за день
        /// </summary>
        public float CalculateDailyProfit(int day)
        {
            var dayData = GetDayProgression(day);
            return dayData.coinsFromTaps + dayData.coinsFromOfflineBonus;
        }
        
        /// <summary>
        /// Рассчитать накопленный профит до определенного дня
        /// </summary>
        public float CalculateCumulativeProfit(int upToDay)
        {
            if (upToDay < 1 || upToDay > dailyProgression.Length)
            {
                return 0f;
            }
            
            return dailyProgression[upToDay - 1].cumulativeProfitCoins;
        }
        
        /// <summary>
        /// Рассчитать накопленный опыт до определенного дня
        /// </summary>
        public long CalculateCumulativeExp(int upToDay)
        {
            if (upToDay < 1 || upToDay > dailyProgression.Length)
            {
                return 0;
            }
            
            return dailyProgression[upToDay - 1].cumulativeExp;
        }

        /// <summary>
        /// Валидация конфигурации
        /// </summary>
        private void OnValidate()
        {
            coinsPerTap = Mathf.Max(1, coinsPerTap);
            expPerTap = Mathf.Max(1, expPerTap);
            startCapacityHits = Mathf.Max(1, startCapacityHits);
            cooldownPerHitSec = Mathf.Max(0.1f, cooldownPerHitSec);
            critMultiplier = Mathf.Max(1f, critMultiplier);
            chanceCritPercent = Mathf.Clamp(chanceCritPercent, 0f, 100f);
            avgPlaytimeMinutes = Mathf.Max(1, avgPlaytimeMinutes);
            tapsPerSecond = Mathf.Max(1, tapsPerSecond);
            profitPerHour = Mathf.Max(0, profitPerHour);
            maxHoursOffline = Mathf.Max(0, maxHoursOffline);
            sessionsPerDay = Mathf.Max(1, sessionsPerDay);
        }
    }

    /// <summary>
    /// Данные прогрессии игрока по дням
    /// Соответствует строкам из WattsBalanceMining.csv
    /// </summary>
    [Serializable]
    public class DailyProgressionData
    {
        [Tooltip("День прогрессии (1-30)")]
        public int day;
        
        [Tooltip("Время игры в секундах за сессию")]
        public int playtimeSec = 900;
        
        [Tooltip("Тапов за сессию")]
        public int tapsPerSession = 1950;
        
        [Tooltip("Тапов за день")]
        public int tapsPerDay = 3900;
        
        [Tooltip("Опыт за день")]
        public long expPerDay = 3900;
        
        [Tooltip("Монеты с тапов")]
        public float coinsFromTaps = 3946.8f;
        
        [Tooltip("Монеты с оффлайн бонуса")]
        public float coinsFromOfflineBonus = 3000f;
        
        [Tooltip("Общий профит монет за день")]
        public float profitCoins = 6946.8f;
        
        [Tooltip("Накопленный профит монет")]
        public float cumulativeProfitCoins = 6946.8f;
        
        [Tooltip("Накопленный опыт")]
        public long cumulativeExp = 3900;
    }
}
