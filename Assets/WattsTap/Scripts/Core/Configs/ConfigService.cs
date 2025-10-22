using System;
using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Core.Configs
{
    public class ConfigService : MonoBehaviour, IConfigService
    {
        private readonly Dictionary<Type, Dictionary<string, BaseConfig>> _allConfigs = new();

        [SerializeField] private BaseConfig[] configs;

        public int InitializationOrder => 1;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            foreach (var config in configs)
            {
                RegisterConfig(config.name, config);
            }

            IsInitialized = true;
        }


        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            _allConfigs.Clear();
            IsInitialized = false;
        }

        public T GetConfig<T>(string key = "default") where T : BaseConfig
        {
            Type type = typeof(T);

            if (!_allConfigs.TryGetValue(type, out var typedDict))
            {
                throw new Exception($"No configs of type {type.Name} registered.");
            }

            if (typedDict.TryGetValue(key, out var config))
            {
                return config as T;
            }

            throw new Exception($"Config of type {type.Name} with key '{key}' not found.");

        }

        public void RegisterConfig<T>(string key, T config) where T : BaseConfig
        {
            Type type = config.GetType();
            if (!_allConfigs.ContainsKey(type))
            {
                _allConfigs[type] = new Dictionary<string, BaseConfig>();
            }

            if (_allConfigs[type].ContainsKey(key))
            {
                throw new Exception($"Config of type {type.Name} with key '{key}' already registered.");
            }

            _allConfigs[type][key] = config;
        }
    }
}