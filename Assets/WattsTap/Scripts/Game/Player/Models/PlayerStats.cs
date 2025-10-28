using System;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Статистика игрока (аналитика и достижения)
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        /// <summary>
        /// Общее количество тапов за всё время
        /// </summary>
        public long totalTaps;
        
        /// <summary>
        /// Общее время в игре (в секундах)
        /// </summary>
        public long totalPlayTimeSeconds;
        
        /// <summary>
        /// Текущий пассивный доход в час
        /// </summary>
        public long incomePerHour;
        
        /// <summary>
        /// Доход за один тап
        /// </summary>
        public long incomePerTap;
        
        /// <summary>
        /// Количество друзей (рефералов)
        /// </summary>
        public int friendsCount;
        
        /// <summary>
        /// Количество купленных апгрейдов
        /// </summary>
        public int upgradesPurchased;
        
        /// <summary>
        /// Количество открытых сундуков
        /// </summary>
        public int chestsOpened;
        
        /// <summary>
        /// Текущая позиция в турнире
        /// </summary>
        public int tournamentRank;
        
        /// <summary>
        /// Лучшая позиция в турнире за всё время
        /// </summary>
        public int bestTournamentRank;
        
        /// <summary>
        /// Время последнего входа в игру (UTC)
        /// </summary>
        public DateTime lastLoginTime;
        
        /// <summary>
        /// Время выхода из игры (UTC) для расчёта оффлайн-дохода
        /// </summary>
        public DateTime lastLogoutTime;

        public PlayerStats()
        {
            totalTaps = 0;
            totalPlayTimeSeconds = 0;
            incomePerHour = 0;
            incomePerTap = 1;
            friendsCount = 0;
            upgradesPurchased = 0;
            chestsOpened = 0;
            tournamentRank = 0;
            bestTournamentRank = 0;
            lastLoginTime = DateTime.UtcNow;
            lastLogoutTime = DateTime.UtcNow;
        }
    }
}
