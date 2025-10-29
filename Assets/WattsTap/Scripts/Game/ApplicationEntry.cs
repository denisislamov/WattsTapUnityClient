using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.Configs;
using WattsTap.Core.UI;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Core
{
    public class ApplicationEntry : MonoBehaviour, IApplicationEntry
    {
        [SerializeField] private ConfigService _stageConfigService;
        
        [Header("UI")]
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private UIHost _overlayRoot;
        
        private ServiceLocator _serviceManager;
        public ServiceLocator ServiceManager => _serviceManager;

        private void Awake()
        {
            _serviceManager = new ServiceLocator(this);
            _serviceManager.OnPostInitialize += OnPostInitialize;
            
            ServiceLocator.Register<IConfigService>(_stageConfigService);
            _stageConfigService.Initialize();
            
            
            ServiceLocator.Register<IUIService>(new UIService(_uiRoot, _overlayRoot));

            // Register input and tap services so they are available at runtime. They will be initialized by ServiceLocator.InitializeAll() below.
            ServiceLocator.Register<IInputService>(new InputService());
            ServiceLocator.Register<ITapControllerService>(new TapControllerService());
            
            _serviceManager.InitializeAll();
        }

        private void OnPostInitialize()
        {
            var uiService = ServiceLocator.Get<IUIService>();
            uiService.Open(UIConstants.MainMenu);
        }
    }
}