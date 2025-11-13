using UnityEngine;

namespace WattsTap.Core.DataPersistence
{
    /// <summary>
    /// Реализация сохранения данных через REST API
    /// TODO: Реализовать после подключения API
    /// </summary>
    public class RestApiDataPersistenceService : IDataPersistenceService
    {
        public int InitializationOrder => 5;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            Debug.Log("<color=green>[RestApiDataPersistenceService] Initialized (stub implementation)</color>");
        }

        public void Shutdown()
        {
            IsInitialized = false;
            Debug.Log("[RestApiDataPersistenceService] Shutdown");
        }

        public void SaveData<T>(string key, T data) where T : class
        {
            // TODO: Implement REST API save
            Debug.LogWarning($"[RestApiDataPersistenceService] SaveData not implemented yet for key: {key}");
        }

        public T LoadData<T>(string key) where T : class
        {
            // TODO: Implement REST API load
            Debug.LogWarning($"[RestApiDataPersistenceService] LoadData not implemented yet for key: {key}");
            return null;
        }

        public bool HasData(string key)
        {
            // TODO: Implement REST API check
            Debug.LogWarning($"[RestApiDataPersistenceService] HasData not implemented yet for key: {key}");
            return false;
        }

        public void DeleteData(string key)
        {
            // TODO: Implement REST API delete
            Debug.LogWarning($"[RestApiDataPersistenceService] DeleteData not implemented yet for key: {key}");
        }

        public void ClearAll()
        {
            // TODO: Implement REST API clear
            Debug.LogWarning("[RestApiDataPersistenceService] ClearAll not implemented yet");
        }
    }
}

