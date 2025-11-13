using UnityEngine;

namespace WattsTap.Core.DataPersistence
{
    /// <summary>
    /// Реализация сохранения данных через PlayerPrefs
    /// </summary>
    public class PlayerPrefsDataPersistenceService : IDataPersistenceService
    {
        public int InitializationOrder => 5;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;
            Debug.Log("<color=green>[PlayerPrefsDataPersistenceService] Initialized</color>");
        }

        public void Shutdown()
        {
            PlayerPrefs.Save();
            IsInitialized = false;
            Debug.Log("[PlayerPrefsDataPersistenceService] Shutdown");
        }

        public void SaveData<T>(string key, T data) where T : class
        {
            if (data == null)
            {
                Debug.LogWarning($"[PlayerPrefsDataPersistenceService] Attempted to save null data for key: {key}");
                return;
            }

            try
            {
                string json = JsonUtility.ToJson(data, true);
                PlayerPrefs.SetString(key, json);
                PlayerPrefs.Save();
                Debug.Log($"[PlayerPrefsDataPersistenceService] Saved data for key: {key}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerPrefsDataPersistenceService] Failed to save data for key {key}: {e.Message}");
            }
        }

        public T LoadData<T>(string key) where T : class
        {
            try
            {
                string json = PlayerPrefs.GetString(key, string.Empty);
                
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"[PlayerPrefsDataPersistenceService] No data found for key: {key}");
                    return null;
                }

                T data = JsonUtility.FromJson<T>(json);
                Debug.Log($"[PlayerPrefsDataPersistenceService] Loaded data for key: {key}");
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerPrefsDataPersistenceService] Failed to load data for key {key}: {e.Message}");
                return null;
            }
        }

        public bool HasData(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteData(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Debug.Log($"[PlayerPrefsDataPersistenceService] Deleted data for key: {key}");
            }
        }

        public void ClearAll()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[PlayerPrefsDataPersistenceService] Cleared all data");
        }
    }
}

