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
        /// Игровые ресурсы (Watts, Energy, XP)
        /// </summary>
        public PlayerResources resources;
        
        public PlayerData()
        {
            playerId = Guid.NewGuid().ToString();
            nickname = "Player";
            level = 1;
            avatarUrl = string.Empty;
            telegramUserId = 0;
            resources = new PlayerResources();
        }
    }
}
