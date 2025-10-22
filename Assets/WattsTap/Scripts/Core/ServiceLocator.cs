using System;
using System.Collections.Generic;
using System.Linq;

namespace WattsTap.Core
{
    public class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> _services = new();
        public event Action OnPreInitialize;
        public event Action OnPostInitialize;
        
        private static IApplicationEntry _applicationEntry;
        public static ServiceLocator Instance => _applicationEntry?.ServiceManager;

        
        public ServiceLocator(IApplicationEntry applicationEntry)
        {
            _applicationEntry = applicationEntry;
        }
        
        public static void Register<T>(T service) where T : IService
        {
            var type = typeof(T);
            if (!_services.TryAdd(type, service))
            {
                throw new Exception($"Service {type} is already registered.");
            }
        }

        public static void Override<T>(T service) where T : IService
        {
            var type = typeof(T);
            _services[type] = service;
        }

        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }
            throw new Exception($"Service {type} is not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var instance))
            {
                service = instance as T;
                return true;
            }

            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            _services.Remove(type);
        }

        public static void Clear()
        {
            _services.Clear();
        }
        
        public void InitializeAll()
        {
            OnPreInitialize?.Invoke();
            foreach (var service in _services.Values.OrderBy(s => s.InitializationOrder))
            {
                service.Initialize();
                UnityEngine.Debug.Log($"<color=#00AA00>Initialized service {service.GetType().Name}</color>");
            }
            
            OnPostInitialize?.Invoke();
        }
        
        public void ShutdownAll()
        {
            foreach (var service in _services.Values.OrderByDescending(s => s.InitializationOrder))
            {
                service.Shutdown();
                UnityEngine.Debug.Log($"<color=#008800>Shutdown service {service.GetType().Name}</color>");
            }
            Clear();
        }
        
        public void InitializeUninitialized()
        {
            foreach (var service in _services.Values.Where(s => !s.IsInitialized).OrderBy(s => s.InitializationOrder))
            {
                service.Initialize();
                UnityEngine.Debug.Log($"<color=#00AA00>Initialized service {service.GetType().Name}</color>");
            }
        }
    }
}
