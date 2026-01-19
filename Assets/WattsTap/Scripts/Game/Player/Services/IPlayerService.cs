using System;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Интерфейс сервиса для управления данными игрока
    /// </summary>
    public interface IPlayerService : Core.IService
    {
        /// <summary>
        /// Событие изменения данных игрока
        /// </summary>
        event Action<PlayerData> OnPlayerDataChanged;
        
        /// <summary>
        /// Событие изменения ресурсов игрока
        /// </summary>
        event Action<PlayerResources> OnResourcesChanged;
        
        /// <summary>
        /// Событие повышения уровня
        /// </summary>
        event Action<int> OnLevelUp;
        
        /// <summary>
        /// Менеджер ресурсов игрока
        /// </summary>
        public IResourceManager ResourceManager { get; }
        
        /// <summary>
        /// Событие изменения энергии
        /// </summary>
        event Action<int, int> OnEnergyChanged; // current, max
        
        /// <summary>
        /// Получить текущие данные игрока
        /// </summary>
        PlayerData GetPlayerData();
        
        /// <summary>
        /// Добавить Watts
        /// </summary>
        void AddWatts(long amount);
        
        /// <summary>
        /// Потратить Watts
        /// </summary>
        bool SpendWatts(long amount);
        
        /// <summary>
        /// Добавить опыт
        /// </summary>
        void AddExperience(long amount);
        
        
        /// <summary>
        /// Выполнить тап (использовать энергию и добавить Watts)
        /// </summary>
        bool PerformTap();

        public int IncomePerTap { get; }
        public int ExperiencePerTap { get; }
        public int EnergyCostPerTap { get; }
        public float IncomeMultiplier { get; }
        
        /// <summary>
        /// Загрузить данные игрока с сервера
        /// </summary>
        void LoadFromServer(int level, long watts, long currentXp, long totalXp);
        
        /// <summary>
        /// Сбросить прогресс игрока
        /// </summary>
        void ResetProgress();
    }
}
