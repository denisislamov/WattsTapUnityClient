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
    }
}

