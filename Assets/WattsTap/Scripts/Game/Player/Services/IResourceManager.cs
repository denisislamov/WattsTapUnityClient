using System;

namespace WattsTap.Game.Player
{
    /// <summary>
    /// Интерфейс менеджера ресурсов игрока
    /// Управляет добавлением, тратой и валидацией ресурсов
    /// </summary>
    public interface IResourceManager
    {
        /// <summary>
        /// Событие изменения ресурса
        /// </summary>
        event Action<ResourceType, long, long> OnResourceChanged; // type, previousValue, newValue
        
        /// <summary>
        /// Событие транзакции ресурса
        /// </summary>
        event Action<ResourceTransaction> OnResourceTransaction;
        
        /// <summary>
        /// Получить текущее значение ресурса
        /// </summary>
        long GetResource(ResourceType type);
        
        /// <summary>
        /// Проверить, достаточно ли ресурса
        /// </summary>
        bool HasEnough(ResourceType type, long amount);
        
        /// <summary>
        /// Добавить ресурс
        /// </summary>
        ResourceTransaction AddResource(ResourceType type, long amount, bool notifyChange = true);
        
        /// <summary>
        /// Потратить ресурс (с проверкой достаточности)
        /// </summary>
        ResourceTransaction SpendResource(ResourceType type, long amount, bool notifyChange = true);
        
        /// <summary>
        /// Установить значение ресурса напрямую (для загрузки данных)
        /// </summary>
        void SetResource(ResourceType type, long value, bool notifyChange = false);
        
        /// <summary>
        /// Попытаться потратить несколько ресурсов атомарно
        /// </summary>
        bool TrySpendMultiple(params (ResourceType type, long amount)[] costs);
        
        /// <summary>
        /// Получить максимальное значение ресурса (для энергии и т.д.)
        /// </summary>
        long GetMaxResource(ResourceType type);
        
        /// <summary>
        /// Установить максимальное значение ресурса
        /// </summary>
        void SetMaxResource(ResourceType type, long maxValue);
    }
}

