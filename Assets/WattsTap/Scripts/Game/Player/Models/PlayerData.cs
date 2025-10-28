using System;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Основные данные игрок��
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        /// <summary>
        /// Уникальный ID игрока
        /// </summary>
        public string playerId;
        
        /// <summary>
        /// Никнейм игрока
        /// </summary>
        public string nickname;
        
        /// <summary>
        /// Текущий уровень игрока
        /// </summary>
        public int level;
        
        /// <summary>
        /// URL аватара игрока
        /// </summary>
        public string avatarUrl;
        
        /// <summary>
        /// Telegram User ID
        /// </summary>
        public long telegramUserId;
        
        /// <summary>
        /// TON Wallet адрес (если подключен)
        /// </summary>
        public string tonWalletAddress;
        
        /// <summary>
        /// Игровые ресурсы (Watts, Energy, XP)
        /// </summary>
        public PlayerResources resources;
        
        /// <summary>
        /// Статистика игрока
        /// </summary>
        public PlayerStats stats;
        
        /// <summary>
        /// Инвентарь игрока
        /// </summary>
        public InventoryData inventory;
        
        /// <summary>
        /// Дата создания аккаунта (UTC)
        /// </summary>
        public DateTime createdAt;
        
        /// <summary>
        /// Дата последнего обновления данных (UTC)
        /// </summary>
        public DateTime updatedAt;
        
        /// <summary>
        /// Количество дней подряд входа в игру (для Daily Bonus)
        /// </summary>
        public int dailyLoginStreak;
        
        /// <summary>
        /// Дата последнего получения Daily Bonus
        /// </summary>
        public DateTime lastDailyBonusDate;

        public PlayerData()
        {
            playerId = Guid.NewGuid().ToString();
            nickname = "Player";
            level = 1;
            avatarUrl = string.Empty;
            telegramUserId = 0;
            tonWalletAddress = string.Empty;
            resources = new PlayerResources();
            stats = new PlayerStats();
            inventory = new InventoryData();
            createdAt = DateTime.UtcNow;
            updatedAt = DateTime.UtcNow;
            dailyLoginStreak = 0;
            lastDailyBonusDate = DateTime.MinValue;
        }
    }
}
