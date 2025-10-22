using UnityEngine;

namespace WattsTap.Core
{
    public class ApplicationEntry : MonoBehaviour, IApplicationEntry
    {
        private ServiceLocator _serviceManager;
        public ServiceLocator ServiceManager => _serviceManager;

        private void Awake()
        {
            _serviceManager = new ServiceLocator(this);
            _serviceManager.OnPostInitialize += OnPostInitialize;
        }

        private void OnPostInitialize()
        {
        }
    }
}
