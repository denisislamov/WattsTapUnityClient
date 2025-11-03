using System;
using System.Collections.Generic;

namespace WattsTap.Core
{
    public class SharedDataService : ISharedDataService
    {
        private readonly Dictionary<string, object> _sharedDataStore = new();
        public int InitializationOrder => 1000;
        public bool IsInitialized { get; private set; }
        
        public event Action<string> OnDataUpdated;
        
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
        
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            foreach (var key in _sharedDataStore.Keys)
            {
                if (_sharedDataStore[key] is ISharedData<object> sharedData)
                {
                    sharedData.SetData(null);
                }
            }
            
            _sharedDataStore.Clear();
            IsInitialized = false;
        }
        
        public void SetData<T>(string key, T data)
        {
            _sharedDataStore[key] = new SharedData<T>(data);
            OnDataUpdated?.Invoke(key);
        }

        public bool TryGetData<T>(string key, out T data)
        {
            if (_sharedDataStore.TryGetValue(key, out var sharedData) && sharedData is ISharedData<T> typedData)
            {
                data = typedData.GetData();
                return true;
            }
            
            data = default;
            return false;
        }
    }
}