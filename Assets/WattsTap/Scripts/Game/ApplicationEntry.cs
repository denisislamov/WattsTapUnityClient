using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.Configs;
using WattsTap.Core.DataPersistence;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Scripts.Game.Mining;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;

namespace WattsTap.Core
{
    public class ApplicationEntry : MonoBehaviour, IApplicationEntry
    {
        /// <summary>
        /// Тип сервиса сохранения данных
        /// </summary>
        public enum DataPersistenceType
        {
            PlayerPrefs,
            RestAPI
        }
        
        [SerializeField] private ConfigService _stageConfigService;
        
        [Header("Data Persistence")]
        [SerializeField] private DataPersistenceType _dataPersistenceType = DataPersistenceType.PlayerPrefs;
        
        [Header("UI")]
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private UIHost _overlayRoot;
        
        [Header("Telegram")]
        [SerializeField] private TelegramService _telegramService;
        
        private SharedDataService _sharedDataService;
        private ServiceLocator _serviceManager;
        public ServiceLocator ServiceManager => _serviceManager;
        
        private void Awake()
        {
            _serviceManager = new ServiceLocator(this);
            _serviceManager.OnPostInitialize += OnPostInitialize;
            
            // Config Service
            ServiceLocator.Register<IConfigService>(_stageConfigService);
            _stageConfigService.Initialize();
            
            // Data Persistence Service (выбор реализации)
            RegisterDataPersistenceService();
            
            // Core Services
            ServiceLocator.Register<IUIService>(new UIService(_uiRoot, _overlayRoot));
            ServiceLocator.Register<IPlayerService>(new PlayerService());
            ServiceLocator.Register<IInputService>(new InputService());
            ServiceLocator.Register<ITapControllerService>(new TapControllerService());
            ServiceLocator.Register<ITelegramService>(_telegramService);
            
            // Mining Balance Service
            ServiceLocator.Register<IMiningBalanceService>(new MiningBalanceService());
            
            // Shared Data Service
            _sharedDataService = new SharedDataService();
            ServiceLocator.Register<ISharedDataService>(_sharedDataService);
            
            _serviceManager.InitializeAll();
        }

        /// <summary>
        /// Регистрирует выбранный тип сервиса сохранения данных
        /// </summary>
        private void RegisterDataPersistenceService()
        {
            IDataPersistenceService persistenceService;
            
            switch (_dataPersistenceType)
            {
                case DataPersistenceType.PlayerPrefs:
                    persistenceService = new PlayerPrefsDataPersistenceService();
                    Debug.Log("<color=cyan>[ApplicationEntry] Using PlayerPrefs for data persistence</color>");
                    break;
                
                case DataPersistenceType.RestAPI:
                    persistenceService = new RestApiDataPersistenceService();
                    Debug.Log("<color=cyan>[ApplicationEntry] Using REST API for data persistence (stub)</color>");
                    break;
                
                default:
                    persistenceService = new PlayerPrefsDataPersistenceService();
                    Debug.LogWarning("[ApplicationEntry] Unknown persistence type, defaulting to PlayerPrefs");
                    break;
            }
            
            ServiceLocator.Register<IDataPersistenceService>(persistenceService);
        }

        private void OnPostInitialize()
        {
            var uiService = ServiceLocator.Get<IUIService>();
            
            if (string.IsNullOrEmpty(_telegramService.InitData))
            {
                _telegramService.OnReceivedInitData += TelegramServiceOnReceivedInitData;
            }
            else
            {
                TelegramServiceOnReceivedInitData(_telegramService.InitData);
            }
            
            uiService.Open(UIConstants.MainMenu);
        }
        
        private void TelegramServiceOnReceivedInitData(string initData)
        {
            var user = TelegramService.ParseUserFromInitData(initData);
            
            if (user == null)
            {
                Debug.LogWarning("Failed to parse user from initData, creating fallback user");
                user = new TelegramService.User();
                
                if (!long.TryParse(_telegramService.Id, out user.id))
                {
                    user.id = -1;
                }
                
                user.username = _telegramService.UserName;
            }
            
            _sharedDataService.SetData(SharedDataConstants.TelegramUser, user);
            _sharedDataService.SetData(SharedDataConstants.TelegramChatId, user.id);
            
            Debug.Log($"Telegram user: {user.first_name} {user.last_name} (@{user.username}), ID: {user.id}");
        }
    }
}