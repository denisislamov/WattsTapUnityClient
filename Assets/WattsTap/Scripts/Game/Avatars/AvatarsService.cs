using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.React;
using WattsTap.Game.Player;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// Service for managing avatars.
    /// Handles avatar configurations, selection, purchase, and Telegram avatar loading.
    /// </summary>
    public class AvatarsService : MonoBehaviour, IAvatarsService
    {
        private const string TelegramAvatarIdConst = "telegram_avatar";
        private const string CurrentAvatarPlayerPrefsKey = "current_avatar_id";
        private const string UnlockedAvatarsPlayerPrefsKey = "unlocked_avatars";
        
        [Header("Avatar Configurations")]
        [SerializeField] private AvatarConfig[] _avatarConfigs;
        
        [Header("Default Avatar")]
        [SerializeField] private Sprite _defaultAvatarSprite;
        
        private Dictionary<string, AvatarConfig> _avatarConfigsMap;
        private string _currentAvatarId;
        private Sprite _telegramAvatarSprite;
        private bool _isTelegramAvatarLoaded;
        private HashSet<string> _unlockedAvatars;
        
        public event Action<string> OnAvatarChanged;
        public event Action<Sprite> OnTelegramAvatarLoaded;
        public event Action<string, AvatarPurchaseResult> OnAvatarPurchased;
        
        public int InitializationOrder => 100;
        public bool IsInitialized { get; private set; }
        
        public string TelegramAvatarId => TelegramAvatarIdConst;
        public bool IsTelegramAvatarLoaded => _isTelegramAvatarLoaded;
        
        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }
            
            InitializeAvatarConfigs();
            InitializeUnlockedAvatars();
            LoadCurrentAvatarFromPrefs();
            LoadTelegramAvatar();
            
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }
            
            _avatarConfigsMap?.Clear();
            _unlockedAvatars?.Clear();
            _telegramAvatarSprite = null;
            _isTelegramAvatarLoaded = false;
            
            IsInitialized = false;
        }
        
        private void InitializeAvatarConfigs()
        {
            _avatarConfigsMap = new Dictionary<string, AvatarConfig>();
            
            if (_avatarConfigs != null)
            {
                foreach (var config in _avatarConfigs)
                {
                    if (config != null && !string.IsNullOrEmpty(config.AvatarId))
                    {
                        _avatarConfigsMap[config.AvatarId] = config;
                    }
                }
            }
        }
        
        private void InitializeUnlockedAvatars()
        {
            _unlockedAvatars = new HashSet<string>();
            
            // Telegram avatar is always unlocked
            _unlockedAvatars.Add(TelegramAvatarIdConst);
            
            // Add avatars that are unlocked by default
            if (_avatarConfigs != null)
            {
                foreach (var config in _avatarConfigs)
                {
                    if (config != null && config.IsUnlockedByDefault)
                    {
                        _unlockedAvatars.Add(config.AvatarId);
                    }
                }
            }
            
            // Load unlocked avatars from PlayerPrefs
            LoadUnlockedAvatarsFromPrefs();
            
            // TODO: Load unlocked avatars from server when API is available
            // SyncUnlockedAvatarsFromServer();
        }
        
        private void LoadCurrentAvatarFromPrefs()
        {
            _currentAvatarId = PlayerPrefs.GetString(CurrentAvatarPlayerPrefsKey, TelegramAvatarIdConst);
            
            // Validate that the avatar is unlocked
            if (!IsAvatarUnlocked(_currentAvatarId))
            {
                _currentAvatarId = TelegramAvatarIdConst;
            }
        }
        
        private void SaveCurrentAvatarToPrefs()
        {
            PlayerPrefs.SetString(CurrentAvatarPlayerPrefsKey, _currentAvatarId);
            PlayerPrefs.Save();
        }
        
        private void LoadTelegramAvatar()
        {
            _telegramAvatarSprite = _defaultAvatarSprite;
            _isTelegramAvatarLoaded = false;
            
            // Try to get user data from SharedDataService
            if (ServiceLocator.TryGet<ISharedDataService>(out var sharedDataService))
            {
                if (sharedDataService.TryGetData(SharedDataConstants.TelegramUser, out TelegramService.User user))
                {
                    if (!string.IsNullOrEmpty(user.photo_url))
                    {
                        StartCoroutine(LoadAvatarFromUrl(user.photo_url));
                        return;
                    }
                }
            }
            
            // If no photo URL, use default
            _isTelegramAvatarLoaded = true;
            Debug.Log("[AvatarsService] No Telegram photo URL, using default avatar");
        }
        
        private IEnumerator LoadAvatarFromUrl(string url)
        {
            Debug.Log($"[AvatarsService] Loading Telegram avatar from: {url}");
            
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    _telegramAvatarSprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                    _isTelegramAvatarLoaded = true;
                    
                    Debug.Log("[AvatarsService] Telegram avatar loaded successfully");
                    OnTelegramAvatarLoaded?.Invoke(_telegramAvatarSprite);
                }
                else
                {
                    Debug.LogWarning($"[AvatarsService] Failed to load Telegram avatar: {request.error}");
                    _telegramAvatarSprite = _defaultAvatarSprite;
                    _isTelegramAvatarLoaded = true;
                }
            }
        }
        
        public IReadOnlyList<AvatarConfig> GetAllAvatarConfigs()
        {
            return _avatarConfigs ?? Array.Empty<AvatarConfig>();
        }
        
        public IReadOnlyList<AvatarConfig> GetSortedAvatarConfigs()
        {
            if (_avatarConfigs == null || _avatarConfigs.Length == 0)
            {
                return Array.Empty<AvatarConfig>();
            }
            
            return _avatarConfigs
                .Where(c => c != null)
                .OrderBy(c => GetUnlockTypeSortOrder(c.UnlockType))
                .ThenBy(c => c.UnlockType == AvatarUnlockType.Level ? c.RequiredLevel : int.MaxValue)
                .ThenBy(c => c.UnlockType == AvatarUnlockType.Coins ? c.CoinPrice : long.MaxValue)
                .ThenBy(c => c.UnlockType == AvatarUnlockType.BTN ? c.BtnPrice : long.MaxValue)
                .ToList();
        }
        
        private static int GetUnlockTypeSortOrder(AvatarUnlockType unlockType)
        {
            return unlockType switch
            {
                AvatarUnlockType.Free => 0,
                AvatarUnlockType.Level => 1,
                AvatarUnlockType.Coins => 2,
                AvatarUnlockType.BTN => 3,
                _ => 4
            };
        }
        
        public AvatarConfig GetAvatarConfig(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return null;
            }
            
            _avatarConfigsMap.TryGetValue(avatarId, out var config);
            return config;
        }
        
        public string GetCurrentAvatarId()
        {
            return _currentAvatarId;
        }
        
        public Sprite GetCurrentAvatarSprite()
        {
            if (_currentAvatarId == TelegramAvatarIdConst)
            {
                return GetTelegramAvatarSprite();
            }
            
            var config = GetAvatarConfig(_currentAvatarId);
            return config != null ? config.AvatarSprite : _defaultAvatarSprite;
        }
        
        public bool SelectAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                Debug.LogWarning("[AvatarsService] Cannot select avatar with empty ID");
                return false;
            }
            
            if (!IsAvatarUnlocked(avatarId))
            {
                Debug.LogWarning($"[AvatarsService] Avatar '{avatarId}' is locked");
                return false;
            }
            
            // Check if avatar exists (either Telegram or in configs)
            if (avatarId != TelegramAvatarIdConst && !_avatarConfigsMap.ContainsKey(avatarId))
            {
                Debug.LogWarning($"[AvatarsService] Avatar '{avatarId}' not found");
                return false;
            }
            
            if (_currentAvatarId == avatarId)
            {
                Debug.Log($"[AvatarsService] Avatar '{avatarId}' is already selected");
                return true;
            }
            
            _currentAvatarId = avatarId;
            SaveCurrentAvatarToPrefs();
            
            Debug.Log($"[AvatarsService] Avatar changed to '{avatarId}'");
            OnAvatarChanged?.Invoke(_currentAvatarId);
            
            return true;
        }
        
        public bool IsAvatarUnlocked(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return false;
            }
            
            return _unlockedAvatars.Contains(avatarId);
        }
        
        public Sprite GetTelegramAvatarSprite()
        {
            return _telegramAvatarSprite ?? _defaultAvatarSprite;
        }
        
        /// <summary>
        /// Получить список разблокированных аватаров
        /// </summary>
        public IReadOnlyCollection<string> GetUnlockedAvatars()
        {
            return _unlockedAvatars;
        }
        
        /// <summary>
        /// Проверяет, можно ли купить аватар за монеты при текущем уровне игрока
        /// </summary>
        public bool CanPurchaseAvatarWithCoins(string avatarId, int playerLevel, long playerCoins)
        {
            if (string.IsNullOrEmpty(avatarId))
                return false;
                
            if (IsAvatarUnlocked(avatarId))
                return false;
                
            var config = GetAvatarConfig(avatarId);
            if (config == null)
                return false;
                
            if (!config.CanBuyWithCoins)
                return false;
                
            if (!config.IsAvailableAtLevel(playerLevel))
                return false;
                
            return playerCoins >= config.CoinPrice;
        }
        
        /// <summary>
        /// Проверяет, можно ли разблокировать аватар по уровню
        /// </summary>
        public bool CanUnlockByLevel(string avatarId, int playerLevel)
        {
            if (string.IsNullOrEmpty(avatarId))
                return false;
                
            if (IsAvatarUnlocked(avatarId))
                return false;
                
            var config = GetAvatarConfig(avatarId);
            if (config == null)
                return false;
                
            if (config.UnlockType != AvatarUnlockType.Level)
                return false;
                
            return config.IsAvailableAtLevel(playerLevel);
        }
        
        /// <summary>
        /// Покупает аватар за монеты (Watts)
        /// </summary>
        public AvatarPurchaseResult PurchaseAvatarWithCoins(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                Debug.LogWarning("[AvatarsService] Cannot purchase avatar with empty ID");
                return AvatarPurchaseResult.AvatarNotFound;
            }
            
            if (IsAvatarUnlocked(avatarId))
            {
                Debug.Log($"[AvatarsService] Avatar '{avatarId}' is already unlocked");
                return AvatarPurchaseResult.AlreadyUnlocked;
            }
            
            var config = GetAvatarConfig(avatarId);
            if (config == null)
            {
                Debug.LogWarning($"[AvatarsService] Avatar config '{avatarId}' not found");
                return AvatarPurchaseResult.AvatarNotFound;
            }
            
            if (!config.CanBuyWithCoins)
            {
                Debug.LogWarning($"[AvatarsService] Avatar '{avatarId}' cannot be purchased with coins");
                return AvatarPurchaseResult.Error;
            }
            
            // Get player service to check level and spend coins
            if (!ServiceLocator.TryGet<IPlayerService>(out var playerService))
            {
                Debug.LogError("[AvatarsService] IPlayerService not found");
                return AvatarPurchaseResult.Error;
            }
            
            var playerData = playerService.GetPlayerData();
            
            // Check level requirement
            if (!config.IsAvailableAtLevel(playerData.level))
            {
                Debug.Log($"[AvatarsService] Player level {playerData.level} is below required {config.RequiredLevel} for avatar '{avatarId}'");
                return AvatarPurchaseResult.LevelTooLow;
            }
            
            // Check if player has enough coins
            if (playerData.resources.watts < config.CoinPrice)
            {
                Debug.Log($"[AvatarsService] Not enough coins. Has: {playerData.resources.watts}, Needs: {config.CoinPrice}");
                return AvatarPurchaseResult.NotEnoughCoins;
            }
            
            // Spend coins
            if (!playerService.SpendWatts(config.CoinPrice))
            {
                Debug.LogError($"[AvatarsService] Failed to spend {config.CoinPrice} Watts for avatar '{avatarId}'");
                return AvatarPurchaseResult.Error;
            }
            
            // Unlock avatar
            UnlockAvatarInternal(avatarId);
            
            Debug.Log($"<color=#00FF00>[AvatarsService] Avatar '{avatarId}' purchased for {config.CoinPrice} Watts</color>");
            OnAvatarPurchased?.Invoke(avatarId, AvatarPurchaseResult.Success);
            
            return AvatarPurchaseResult.Success;
        }
        
        /// <summary>
        /// Покупает аватар за BTN токены (пока недоступно)
        /// </summary>
        public AvatarPurchaseResult PurchaseAvatarWithBTN(string avatarId)
        {
            // TODO: Implement BTN purchase when BTN tokens are available
            Debug.LogWarning("[AvatarsService] BTN purchase is not available yet");
            OnAvatarPurchased?.Invoke(avatarId, AvatarPurchaseResult.BTNPurchaseNotAvailable);
            return AvatarPurchaseResult.BTNPurchaseNotAvailable;
        }
        
        /// <summary>
        /// Разблокирует аватар по достижении уровня (бесплатно)
        /// </summary>
        public AvatarPurchaseResult UnlockAvatarByLevel(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return AvatarPurchaseResult.AvatarNotFound;
            }
            
            if (IsAvatarUnlocked(avatarId))
            {
                return AvatarPurchaseResult.AlreadyUnlocked;
            }
            
            var config = GetAvatarConfig(avatarId);
            if (config == null)
            {
                return AvatarPurchaseResult.AvatarNotFound;
            }
            
            if (config.UnlockType != AvatarUnlockType.Level)
            {
                Debug.LogWarning($"[AvatarsService] Avatar '{avatarId}' is not unlockable by level");
                return AvatarPurchaseResult.Error;
            }
            
            // Get player level
            if (!ServiceLocator.TryGet<IPlayerService>(out var playerService))
            {
                Debug.LogError("[AvatarsService] IPlayerService not found");
                return AvatarPurchaseResult.Error;
            }
            
            var playerData = playerService.GetPlayerData();
            
            if (!config.IsAvailableAtLevel(playerData.level))
            {
                Debug.Log($"[AvatarsService] Player level {playerData.level} is below required {config.RequiredLevel} for avatar '{avatarId}'");
                return AvatarPurchaseResult.LevelTooLow;
            }
            
            // Unlock avatar for free
            UnlockAvatarInternal(avatarId);
            
            Debug.Log($"<color=#00FF00>[AvatarsService] Avatar '{avatarId}' unlocked by reaching level {config.RequiredLevel}</color>");
            OnAvatarPurchased?.Invoke(avatarId, AvatarPurchaseResult.Success);
            
            return AvatarPurchaseResult.Success;
        }

        /// <summary>
        /// Unlocks an avatar. Call this when player earns a new avatar.
        /// </summary>
        public void UnlockAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return;
            }
            
            UnlockAvatarInternal(avatarId);
        }
        
        #region Private Methods
        
        private void UnlockAvatarInternal(string avatarId)
        {
            if (_unlockedAvatars.Add(avatarId))
            {
                Debug.Log($"[AvatarsService] Avatar '{avatarId}' unlocked");
                SaveUnlockedAvatarsToPrefs();
                
                // TODO: Sync unlocked avatars to server when API is available
                // SyncUnlockedAvatarsToServer();
            }
        }
        
        private void LoadUnlockedAvatarsFromPrefs()
        {
            string savedAvatars = PlayerPrefs.GetString(UnlockedAvatarsPlayerPrefsKey, string.Empty);
            
            if (!string.IsNullOrEmpty(savedAvatars))
            {
                string[] avatarIds = savedAvatars.Split(',');
                foreach (var id in avatarIds)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _unlockedAvatars.Add(id.Trim());
                    }
                }
                Debug.Log($"[AvatarsService] Loaded {avatarIds.Length} unlocked avatars from PlayerPrefs");
            }
        }
        
        private void SaveUnlockedAvatarsToPrefs()
        {
            // Filter out always-unlocked avatars (telegram, defaults)
            var purchasedAvatars = _unlockedAvatars
                .Where(id => !IsAlwaysUnlocked(id))
                .ToArray();
            
            string avatarsString = string.Join(",", purchasedAvatars);
            PlayerPrefs.SetString(UnlockedAvatarsPlayerPrefsKey, avatarsString);
            PlayerPrefs.Save();
            
            Debug.Log($"[AvatarsService] Saved {purchasedAvatars.Length} purchased avatars to PlayerPrefs");
        }
        
        private bool IsAlwaysUnlocked(string avatarId)
        {
            if (avatarId == TelegramAvatarIdConst)
                return true;
                
            var config = GetAvatarConfig(avatarId);
            return config != null && config.IsUnlockedByDefault;
        }
        
        #endregion
        
        #region Server Sync (TODO)
        
        // TODO: Implement server sync when API endpoints are available
        
        // Синхронизирует разблокированные аватары с сервером (загрузка)
        // private void SyncUnlockedAvatarsFromServer()
        // {
        //     // TODO: Call API to get list of unlocked avatars
        //     // Example: apiService.GetUnlockedAvatars(onSuccess: (avatars) => { ... });
        // }
        
        // Синхронизирует разблокированные аватары с сервером (сохранение)
        // private void SyncUnlockedAvatarsToServer()
        // {
        //     // TODO: Call API to save unlocked avatars
        //     // Example: apiService.SaveUnlockedAvatars(_unlockedAvatars.ToList(), onSuccess: () => { ... });
        // }
        
        #endregion
    }
}

