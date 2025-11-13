namespace WattsTap.Core.DataPersistence
{
    /// <summary>
    /// Интерфейс для сохранения и загрузки данных
    /// </summary>
    public interface IDataPersistenceService : IService
    {
        /// <summary>
        /// Сохранить данные
        /// </summary>
        void SaveData<T>(string key, T data) where T : class;
        
        /// <summary>
        /// Загрузить данные
        /// </summary>
        T LoadData<T>(string key) where T : class;
        
        /// <summary>
        /// Проверить существование данных
        /// </summary>
        bool HasData(string key);
        
        /// <summary>
        /// Удалить данные
        /// </summary>
        void DeleteData(string key);
        
        /// <summary>
        /// Очистить все данные
        /// </summary>
        void ClearAll();
    }
}