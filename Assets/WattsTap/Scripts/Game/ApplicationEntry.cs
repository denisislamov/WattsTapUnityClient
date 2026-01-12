using System;
using System.Collections;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.Configs;
using WattsTap.Core.GameLoop;
using WattsTap.Core.React;
using WattsTap.Core.UI;
using WattsTap.Core.API;
using WattsTap.Core.Services;
using WattsTap.Core.Services.BugReport;
using WattsTap.Core.Telegram;
using WattsTap.Core.Inventory;
using WattsTap.Game.Avatars;
using WattsTap.Game.Player;
using WattsTap.Game.Tap.Services;
using WattsTap.Game.UI;

namespace WattsTap.Core
{
    public class ApplicationEntry : MonoBehaviour, IApplicationEntry
    {
        [SerializeField] private ConfigService _stageConfigService;
        
        [Header("UI")]
        [SerializeField] private Transform _uiRoot;
        [SerializeField] private UIHost _overlayRoot;
        
        [Header("Telegram")]
        [SerializeField] private TelegramService _telegramService;
        [SerializeField] private HapticFeedbackService _hapticFeedbackService;
        
        [Header("Update Service")]       
        [SerializeField] private UpdateService _updateService;
        [SerializeField] private MainMenuThemeManager _mainMenuThemeManager;
        
        [Header("Avatars")]
        [SerializeField] private AvatarsService _avatarsService;
        
        private SharedDataService _sharedDataService;
        private ServiceLocator _serviceManager;
        public ServiceLocator ServiceManager => _serviceManager;
        
        private void Awake()
        {
            _serviceManager = new ServiceLocator(this);
            _serviceManager.OnPostInitialize += OnPostInitialize;
            
            ServiceLocator.Register<IConfigService>(_stageConfigService);
            _stageConfigService.Initialize();
            
            ServiceLocator.Register<IUpdateService>(_updateService);
            ServiceLocator.Register<IUIService>(new UIService(_uiRoot, _overlayRoot));
            ServiceLocator.Register<IPlayerService>(new PlayerService());
            ServiceLocator.Register<IInputService>(new InputService());
            ServiceLocator.Register<ITapControllerService>(new TapControllerService());
            ServiceLocator.Register<ITelegramService>(_telegramService);
            ServiceLocator.Register<IHapticFeedbackService>(_hapticFeedbackService);
            
            ServiceLocator.Register(_mainMenuThemeManager);
            
            _sharedDataService = new SharedDataService();
            ServiceLocator.Register<ISharedDataService>(_sharedDataService);
            
            // Register Referral Services
            ServiceLocator.Register<IReferralAPIService>(new ReferralAPIService());
            ServiceLocator.Register<IReferralService>(new ReferralService());
            
            // Register Avatars Service
            ServiceLocator.Register<IAvatarsService>(_avatarsService);
            
            // Register Bug Report Service
            ServiceLocator.Register<IBugReportService>(new BugReportService());
            
            // Register Inventory Services
            ServiceLocator.Register<ICatalogService>(new CatalogService());
            ServiceLocator.Register<IInventoryService>(new InventoryService());
            
            _serviceManager.InitializeAll();
            
            // Register Orientation Service
            ServiceLocator.Register<IOrientationService>(new OrientationService());
            _serviceManager.InitializeUninitialized();
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
            uiService.Open(UIConstants.WelcomeScreen);
        }
        
        private void TelegramServiceOnReceivedInitData(string initData)
        {
            // Debug: Log all Telegram parameters
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] ========== TELEGRAM DEBUG INFO ==========</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] InitData length: {initData?.Length ?? 0}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] StartParam: {_telegramService.StartParam ?? "null"}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] HasReferralCode: {_telegramService.HasReferralCode}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] CleanReferralCode: {_telegramService.GetCleanReferralCode() ?? "null"}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] AppEnvironment: {_telegramService.AppEnvironment}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] UserId: {_telegramService.Id ?? "null"}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] UserName: {_telegramService.UserName ?? "null"}</color>");
            Debug.Log($"<color=#00FFFF>[ApplicationEntry] ==========================================</color>");
            
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
            
            // Authenticate with referral API
            AuthenticateWithReferralAPI(initData);
        }
        
        private void AuthenticateWithReferralAPI(string initData)
        {
            if (!ServiceLocator.TryGet<IReferralAPIService>(out var apiService))
            {
                Debug.LogWarning("[ApplicationEntry] IReferralAPIService not found, skipping authentication");
                return;
            }
            
            // Check if already authenticated
            if (apiService.IsAuthenticated)
            {
                Debug.Log("[ApplicationEntry] Already authenticated with referral API");
                return;
            }
            
            // Get referral code from TelegramService.StartParam (received from JavaScript)
            // This is more reliable than parsing initData
            string referralCode = _telegramService.GetCleanReferralCode();
            
            // Fallback: try to extract from initData string if StartParam is empty
            if (string.IsNullOrEmpty(referralCode))
            {
                referralCode = ExtractReferralCodeFromInitData(initData);
            }
            
            if (!string.IsNullOrEmpty(referralCode))
            {
                Debug.Log($"<color=#00FF00>[ApplicationEntry] Using referral code: {referralCode}</color>");
            }
            
            StartCoroutine(apiService.Authenticate(
                initData,
                referralCode,
                onSuccess: (response) =>
                {
                    Debug.Log($"<color=#00FF00>[ApplicationEntry] Referral API auth successful. Player ID: {response.player?.playerId}</color>");
                    
                    // Log referral result if present
                    if (response.referral != null && response.referral.applied)
                    {
                        Debug.Log($"<color=#00FF00>[ApplicationEntry] Referral applied! Referrer: {response.referral.referrer?.nickname}, Bonus: {response.referral.bonusForReferrer}</color>");
                    }
                    
                    // Store referral code in ReferralService
                    if (ServiceLocator.TryGet<IReferralService>(out var referralService) && response.player != null)
                    {
                        referralService.SetReferralCodeFromAuth(response.player.referralCode);
                    }
                },
                onError: (error) =>
                {
                    Debug.LogError($"[ApplicationEntry] Referral API auth failed: {error}");
                }
            ));
        }
        
        private string ExtractReferralCodeFromInitData(string initData)
        {
            // initData contains start_param if user opened bot via referral link
            // Format: ...&start_param=REF_ABC123&...
            try
            {
                string decoded = UnityEngine.Networking.UnityWebRequest.UnEscapeURL(initData);
                
                int startParamIndex = decoded.IndexOf("start_param=", StringComparison.Ordinal);
                if (startParamIndex == -1) return null;
                
                int valueStart = startParamIndex + "start_param=".Length;
                int valueEnd = decoded.IndexOf('&', valueStart);
                
                string startParam = valueEnd == -1 
                    ? decoded.Substring(valueStart) 
                    : decoded.Substring(valueStart, valueEnd - valueStart);
                
                // Check if it's a referral code (starts with REF_)
                if (startParam.StartsWith("REF_", StringComparison.OrdinalIgnoreCase))
                {
                    string code = startParam.Substring(4); // Remove "REF_" prefix
                    Debug.Log($"[ApplicationEntry] Found referral code in initData: {code}");
                    return code;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ApplicationEntry] Failed to parse referral code from initData: {ex.Message}");
            }
            
            return null;
        }
    }
}