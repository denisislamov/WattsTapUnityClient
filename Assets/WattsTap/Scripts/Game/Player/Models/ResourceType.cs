namespace WattsTap.Game.Player
{
    /// <summary>
    /// Типы игровых ресурсов
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// Основная игровая валюта (Watts)
        /// </summary>
        Watts = 0,
        
        /// <summary>
        /// Энергия для тапов
        /// </summary>
        Energy = 1,
        
        /// <summary>
        /// Опыт игрока
        /// </summary>
        Experience = 2,
        
        /// <summary>
        /// KiloWatt токены (криптовалюта)
        /// </summary>
        KiloWatt = 3,
        
        /// <summary>
        /// Премиум валюта (для будущего расширения)
        /// </summary>
        Premium = 4,
        
        /// <summary>
        /// Удары (количество доступных тапов)
        /// </summary>
        Hits = 5,
        
        /// <summary>
        /// Опыт до следующего уровня
        /// </summary>
        XpToNextLevel = 6,
        
        /// <summary>
        /// Суммарный опыт на этом уровне
        /// </summary>
        SummXp = 7
    }
}
