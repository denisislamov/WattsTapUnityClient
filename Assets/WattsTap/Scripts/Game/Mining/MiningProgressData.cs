using System;

namespace WattsTap.Scripts.Game.Mining
{
    /// <summary>
    /// Данные прогресса майнинга игрока
    /// </summary>
    [Serializable]
    public class MiningProgressData
    {
        /// <summary>
        /// Текущий день прогрессии (1-30)
        /// </summary>
        public int currentDay = 1;
        
        /// <summary>
        /// Всего тапов сделано
        /// </summary>
        public long totalTaps;
        
        /// <summary>
        /// Всего монет заработано
        /// </summary>
        public long totalCoinsEarned;
        
        /// <summary>
        /// Всего опыта заработано
        /// </summary>
        public long totalExpEarned;
        
        /// <summary>
        /// Тапов сегодня
        /// </summary>
        public int tapsToday;
        
        /// <summary>
        /// Монет заработано сегодня
        /// </summary>
        public float coinsEarnedToday;
        
        /// <summary>
        /// Опыта заработано сегодня
        /// </summary>
        public long expEarnedToday;
        
        /// <summary>
        /// Время последнего тапа
        /// </summary>
        public DateTime lastTapTime;
        
        /// <summary>
        /// Время последнего сброса дня
        /// </summary>
        public DateTime lastDayResetTime;
        
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime createdAt;
        
        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public DateTime updatedAt;

        public MiningProgressData()
        {
            currentDay = 1;
            totalTaps = 0;
            totalCoinsEarned = 0;
            totalExpEarned = 0;
            tapsToday = 0;
            coinsEarnedToday = 0;
            expEarnedToday = 0;
            lastTapTime = DateTime.MinValue;
            lastDayResetTime = DateTime.UtcNow;
            createdAt = DateTime.UtcNow;
            updatedAt = DateTime.UtcNow;
        }
    }
}
