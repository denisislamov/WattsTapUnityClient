using System;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Игровые ресурсы игрока (Watts, Energy, XP)
    /// </summary>
    [Serializable]
    public class PlayerResources
    {
        /// <summary>
        /// Основная валюта игры
        /// </summary>
        public long watts;
        
        /// <summary>
        /// Текущая энергия для тапов
        /// </summary>
        public int currentEnergy;
        
        /// <summary>
        /// Максимальная энергия
        /// </summary>
        public int maxEnergy;
        
        /// <summary>
        /// Текущий опыт
        /// </summary>
        public long currentXP;
        
        /// <summary>
        /// Опыт для следующего уровня
        /// </summary>
        public long xpToNextLevel;
        
        /// <summary>
        /// KiloWatt токены (криптовалюта)
        /// </summary>
        public decimal kiloWattTokens;

        public PlayerResources()
        {
            watts = 0;
            currentEnergy = 100;
            maxEnergy = 100;
            currentXP = 0;
            xpToNextLevel = 100;
            kiloWattTokens = 0;
        }
    }
}
