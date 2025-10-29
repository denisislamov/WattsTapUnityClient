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
        /// Загрузить данные игрока (из PlayerPrefs или сервера)
        /// </summary>
        void LoadPlayerData();
        
        /// <summary>
        /// Сохранить данные игрока
        /// </summary>
        void SavePlayerData();
        
        /// <summary>
        /// Создать нового игрока
        /// </summary>
        void CreateNewPlayer(string nickname, long telegramUserId);
        
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
        /// Использовать энергию для тапа
        /// </summary>
        bool UseEnergy(int amount = 1);
        
        /// <summary>
        /// Восстановить энергию
        /// </summary>
        void RestoreEnergy(int amount);
        
        /// <summary>
        /// Выполнить тап (использовать энергию и добавить Watts)
        /// </summary>
        bool PerformTap();
        
        /// <summary>
        /// Добавить предмет в инвентарь
        /// </summary>
        void AddItem(InventoryItem item);
        
        /// <summary>
        /// Экипировать предмет
        /// </summary>
        bool EquipItem(string itemId);
        
        /// <summary>
        /// Снять предмет
        /// </summary>
        void UnequipItem(ItemType itemType);
        
        /// <summary>
        /// Улучшить предмет
        /// </summary>
        bool UpgradeItem(string itemId, long cost);
        
        /// <summary>
        /// Продать предмет
        /// </summary>
        void SellItem(string itemId);
        
        /// <summary>
        /// Подключить TON кошелёк
        /// </summary>
        void ConnectWallet(string walletAddress);
        
        /// <summary>
        /// Рассчитать и получить оффлайн-доход
        /// </summary>
        long CalculateOfflineIncome();
        
        /// <summary>
        /// Обновить статистику турнира
        /// </summary>
        void UpdateTournamentRank(int rank);
        
        /// <summary>
        /// Пригласить друга
        /// </summary>
        void InviteFriend();
        
        /// <summary>
        /// Получить Daily Bonus
        /// </summary>
        bool ClaimDailyBonus(out int streakDay);
    }
}
