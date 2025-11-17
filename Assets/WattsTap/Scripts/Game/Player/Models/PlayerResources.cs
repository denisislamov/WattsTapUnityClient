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
        /// Текущий опыт
        /// </summary>
        public long currentXP;
        
        /// <summary>
        /// Опыт для следующего уровня
        /// </summary>
        public long xpToNextLevel;

        /// <summary>
        /// Суммарный опыт на этом уровне
        /// </summary>
        public long sumExp;
        
        /// <summary>
        /// KiloWatt токены (криптовалюта)
        /// </summary>
        public decimal kiloWattTokens;

        /// <summary>
        /// УДАРЫ: текущие доступные удары для тапа
        /// </summary>
        public int currentHits;
    }
}
