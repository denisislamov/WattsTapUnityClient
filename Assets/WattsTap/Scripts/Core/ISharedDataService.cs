using System;
using UnityEngine;
using WattsTap.Core;

namespace WattsTap.Core
{
    public interface ISharedDataService : IService
    {
        public void SetData<T>(string key, T data);
        public bool TryGetData<T>(string key, out T data);
        event Action<string> OnDataUpdated;
    }
}
