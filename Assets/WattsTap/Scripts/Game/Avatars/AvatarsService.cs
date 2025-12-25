using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WattsTap.Constants;
using WattsTap.Core;
using WattsTap.Core.React;

namespace WattsTap.Game.Avatars
{
    /// <summary>
    /// Service for managing avatars.
    /// Handles avatar configurations, selection, and Telegram avatar loading.
    /// </summary>
    public class AvatarsService : MonoBehaviour, IAvatarsService
    {
        private const string TelegramAvatarIdConst = "telegram_avatar";
        private const string CurrentAvatarPlayerPrefsKey = "current_avatar_id";
        
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
            
            // TODO: Load unlocked avatars from server/player prefs
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
        /// Unlocks an avatar. Call this when player earns a new avatar.
        /// </summary>
        public void UnlockAvatar(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
            {
                return;
            }
            
            if (_unlockedAvatars.Add(avatarId))
            {
                Debug.Log($"[AvatarsService] Avatar '{avatarId}' unlocked");
                // TODO: Save to server/player prefs
            }
        }
    }
}

