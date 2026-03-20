using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using WattsTap.Constants;
using WattsTap.Core.API;
using WattsTap.Core.Configs;
using WattsTap.Core.Configs.Referral;
using WattsTap.Core.Configs.Telegram;
using WattsTap.Core.React;

namespace WattsTap.Core.Services
{
    /// <summary>
    /// Interface for referral system operations
    /// </summary>
    public interface IReferralService : IService
    {
        /// <summary>User's own referral code</summary>
        string MyReferralCode { get; }
        
        /// <summary>Full invite link for sharing</summary>
        string MyInviteLink { get; }
        
        /// <summary>Number of friends invited via referral</summary>
        int TotalFriendsInvited { get; }
        
        /// <summary>Total bonus earned from referrals</summary>
        long TotalBonusEarned { get; }
        
        /// <summary>Bonus watts per friend</summary>
        int BonusPerFriend { get; }
        
        /// <summary>Whether referral data has been loaded</summary>
        bool IsDataLoaded { get; }
        
        /// <summary>Fired when referral data is loaded from server</summary>
        event Action<MyReferralResponse> OnReferralDataLoaded;
        
        /// <summary>Fired when friends list is loaded from server</summary>
        event Action<FriendsListResponse> OnFriendsListLoaded;
        
        /// <summary>Fired when link is copied to clipboard (for UI feedback)</summary>
        event Action OnLinkCopied;
        
        /// <summary>Load referral data from server</summary>
        void LoadReferralData();
        
        /// <summary>Load friends list from server</summary>
        void LoadFriendsList();
        
        /// <summary>Share invite link via Telegram</summary>
        void ShareInviteLink();
        
        /// <summary>Copy invite link to clipboard</summary>
        void CopyInviteLink();
        
        /// <summary>Set referral data from auth response</summary>
        void SetReferralCodeFromAuth(string referralCode);
    }
    
    /// <summary>
    /// Service for managing referral system
    /// </summary>
    public class ReferralService : IReferralService
    {
        #region JavaScript Interop
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void ShareToTelegram(string url, string text);
        
        [DllImport("__Internal")]
        private static extern void CopyToClipboard(string text);
#endif
        
        #endregion
        
        #region Private Fields
        
        private MyReferralResponse _referralData;
        private FriendsListResponse _friendsData;
        private string _myReferralCode;
        private ReferralConfig _config;
#if UNITY_EDITOR
        private TelegramDebugData _telegramDebugData;
        
        // Enable mock data for testing in Editor without server
        private const bool USE_MOCK_DATA_IN_EDITOR = false;
#endif
        
        #endregion
        
        #region IService Implementation
        
        public int InitializationOrder => 50;
        public bool IsInitialized { get; private set; }
        
        public void Initialize()
        {
            if (IsInitialized) return;
            
            // Load config
            if (ServiceLocator.TryGet<IConfigService>(out var configService))
            {
                _config = configService.GetConfig<ReferralConfig>(ConfigsConstants.ReferralConfig);
                
#if UNITY_EDITOR
                if (USE_MOCK_DATA_IN_EDITOR)
                {
                    _telegramDebugData = configService.GetConfig<TelegramDebugData>(ConfigsConstants.TelegramDebugData);
                    if (_telegramDebugData != null)
                    {
                        // Generate referral code from UserId in debug data
                        _myReferralCode = $"REF{_telegramDebugData.UserId}";
                        Debug.Log($"<color=#FFFF00>[ReferralService] Using referral code from TelegramDebugData: {_myReferralCode} (UserId: {_telegramDebugData.UserId})</color>");
                    }
                }
#endif
            }
            
            if (_config == null)
            {
                Debug.LogError("[ReferralService] ReferralConfig not found! Using default values.");
            }
            
            IsInitialized = true;
            Debug.Log("<color=#00FFFF>[ReferralService] Initialized</color>");
        }
        
        public void Shutdown()
        {
            _referralData = null;
            _friendsData = null;
            IsInitialized = false;
        }
        
        #endregion
        
        #region IReferralService Properties
        
        public string MyReferralCode => _myReferralCode ?? _referralData?.referralCode;
        
        public string MyInviteLink
        {
            get
            {
                if (_referralData?.inviteLink != null)
                    return _referralData.inviteLink;
                
                // Construct link from referral code using config
                if (!string.IsNullOrEmpty(MyReferralCode))
                {
                    if (_config != null)
                        return _config.GetInviteLink(MyReferralCode);
                    
                    // Fallback if config not loaded
                    return $"https://t.me/wattstap_eu_bot?start=REF_{MyReferralCode}";
                }
                
                return null;
            }
        }
        
        public int TotalFriendsInvited => _referralData?.totalFriendsInvited ?? 0;
        public long TotalBonusEarned => _referralData?.totalBonusEarned ?? 0;
        public int BonusPerFriend => _referralData?.bonusPerFriend ?? (_config?.DefaultBonusPerFriend ?? 5000);
        public bool IsDataLoaded => _referralData != null;
        
        #endregion
        
        #region Events
        
        public event Action<MyReferralResponse> OnReferralDataLoaded;
        public event Action<FriendsListResponse> OnFriendsListLoaded;
        public event Action OnLinkCopied;
        
        #endregion
        
        #region Public Methods
        
        public void SetReferralCodeFromAuth(string referralCode)
        {
            _myReferralCode = referralCode;
            Debug.Log($"[ReferralService] Set referral code from auth: {referralCode}");
        }
        
        public void LoadReferralData()
        {
#if UNITY_EDITOR
            if (USE_MOCK_DATA_IN_EDITOR && !ServiceLocator.TryGet<IReferralAPIService>(out _))
            {
                LoadMockReferralData();
                return;
            }
#endif
            
            // Try new CoreServerService first
            if (ServiceLocator.TryGet<ICoreServerService>(out var coreService) && coreService.IsAuthenticated)
            {
                CoroutineRunner.Instance.StartCoroutine(
                    coreService.GetMyReferral(
                        onSuccess: (response) =>
                        {
                            _referralData = response;
                            OnReferralDataLoaded?.Invoke(response);
                            Debug.Log($"<color=#00FF00>[ReferralService] Loaded referral data (core): {response.referralCode}, invited: {response.totalFriendsInvited}</color>");
                        },
                        onError: (error) =>
                        {
                            Debug.LogError($"[ReferralService] Core service referral load failed: {error}");
                            // Fallback to legacy
                            LoadReferralDataLegacy();
                        }
                    )
                );
                return;
            }
            
            LoadReferralDataLegacy();
        }
        
        private void LoadReferralDataLegacy()
        {
            if (!ServiceLocator.TryGet<IReferralAPIService>(out var apiService))
            {
                Debug.LogError("[ReferralService] IReferralAPIService not found");
                return;
            }
            
            CoroutineRunner.Instance.StartCoroutine(
                apiService.GetMyReferral(
                    onSuccess: (response) =>
                    {
                        _referralData = response;
                        OnReferralDataLoaded?.Invoke(response);
                        Debug.Log($"<color=#00FF00>[ReferralService] Loaded referral data: {response.referralCode}, invited: {response.totalFriendsInvited}</color>");
                    },
                    onError: (error) =>
                    {
                        Debug.LogError($"[ReferralService] Failed to load referral data: {error}");
#if UNITY_EDITOR
                        Debug.Log("<color=#FFFF00>[ReferralService] Falling back to mock data</color>");
                        LoadMockReferralData();
#endif
                    }
                )
            );
        }
        
        public void LoadFriendsList()
        {
#if UNITY_EDITOR
            if (USE_MOCK_DATA_IN_EDITOR && !ServiceLocator.TryGet<IReferralAPIService>(out _))
            {
                LoadMockFriendsList();
                return;
            }
#endif
            
            // Try new CoreServerService first
            if (ServiceLocator.TryGet<ICoreServerService>(out var coreService) && coreService.IsAuthenticated)
            {
                CoroutineRunner.Instance.StartCoroutine(
                    coreService.GetFriends(
                        onSuccess: (response) =>
                        {
                            _friendsData = response;
                            OnFriendsListLoaded?.Invoke(response);
                            Debug.Log($"<color=#00FF00>[ReferralService] Loaded {response.totalFriends} friends (core)</color>");
                        },
                        onError: (error) =>
                        {
                            Debug.LogError($"[ReferralService] Core service friends load failed: {error}");
                            LoadFriendsListLegacy();
                        }
                    )
                );
                return;
            }
            
            LoadFriendsListLegacy();
        }
        
        private void LoadFriendsListLegacy()
        {
            if (!ServiceLocator.TryGet<IReferralAPIService>(out var apiService))
            {
                Debug.LogError("[ReferralService] IReferralAPIService not found");
                return;
            }
            
            CoroutineRunner.Instance.StartCoroutine(
                apiService.GetFriends(
                    onSuccess: (response) =>
                    {
                        _friendsData = response;
                        OnFriendsListLoaded?.Invoke(response);
                        Debug.Log($"<color=#00FF00>[ReferralService] Loaded {response.totalFriends} friends</color>");
                    },
                    onError: (error) =>
                    {
                        Debug.LogError($"[ReferralService] Failed to load friends: {error}");
#if UNITY_EDITOR
                        Debug.Log("<color=#FFFF00>[ReferralService] Falling back to mock data</color>");
                        LoadMockFriendsList();
#endif
                    }
                )
            );
        }
        
        public void ShareInviteLink()
        {
            string linkToShare = MyInviteLink;
            
            if (string.IsNullOrEmpty(linkToShare))
            {
                Debug.LogWarning("[ReferralService] No invite link available");
                return;
            }
            
            string shareText = _config?.ShareText ?? "Join me in WattsTap and get bonus coins! ⚡🎮";
            
#if UNITY_WEBGL && !UNITY_EDITOR
            ShareToTelegram(linkToShare, shareText);
            Debug.Log($"[ReferralService] Sharing link via Telegram: {linkToShare}");
#else
            // For Editor - copy to clipboard and open Telegram share URL
            CopyToClipboardEditor(linkToShare);
            
            // Also open the share URL in browser for testing
            string telegramShareUrl = $"https://t.me/share/url?url={UnityEngine.Networking.UnityWebRequest.EscapeURL(linkToShare)}&text={UnityEngine.Networking.UnityWebRequest.EscapeURL(shareText)}";
            Application.OpenURL(telegramShareUrl);
            
            Debug.Log($"<color=#00FF00>[ReferralService] (Editor) Opened Telegram share in browser and copied to clipboard: {linkToShare}</color>");
#endif
        }
        
        public void CopyInviteLink()
        {
            string linkToCopy = MyInviteLink;
            
            if (string.IsNullOrEmpty(linkToCopy))
            {
                Debug.LogWarning("[ReferralService] No invite link available");
                return;
            }
            
#if UNITY_WEBGL && !UNITY_EDITOR
            CopyToClipboard(linkToCopy);
            Debug.Log($"[ReferralService] Copied to clipboard: {linkToCopy}");
#else
            CopyToClipboardEditor(linkToCopy);
#endif
            
            // Fire event for UI feedback
            OnLinkCopied?.Invoke();
        }
        
        #endregion
        
        #region Private Methods
        
#if !UNITY_WEBGL || UNITY_EDITOR
        /// <summary>
        /// Copy text to clipboard in Editor/Standalone using TextEditor
        /// This works reliably outside of OnGUI context
        /// </summary>
        private void CopyToClipboardEditor(string text)
        {
            // Method 1: Using TextEditor (works reliably)
            TextEditor textEditor = new TextEditor();
            textEditor.text = text;
            textEditor.SelectAll();
            textEditor.Copy();
            
            // Method 2: Also set GUIUtility as backup
            GUIUtility.systemCopyBuffer = text;
            
            Debug.Log($"<color=#00FF00>[ReferralService] (Editor) Copied to clipboard: {text}</color>");
        }
#endif
        
#if UNITY_EDITOR
        /// <summary>
        /// Load mock referral data for Editor testing
        /// </summary>
        private void LoadMockReferralData()
        {
            string mockReferralCode = _myReferralCode ?? "TESTCODE";
            string botUsername = _config?.BotUsername ?? "wattstap_eu_bot";
            int bonusPerFriend = _config?.DefaultBonusPerFriend ?? 5000;
            
            _referralData = new MyReferralResponse
            {
                referralCode = mockReferralCode,
                inviteLink = $"https://t.me/{botUsername}?start=REF_{mockReferralCode}",
                bonusPerFriend = bonusPerFriend,
                totalFriendsInvited = 3,
                totalBonusEarned = bonusPerFriend * 3
            };
            
            Debug.Log($"<color=#FFFF00>[ReferralService] Loaded MOCK referral data: {_referralData.referralCode}</color>");
            OnReferralDataLoaded?.Invoke(_referralData);
        }
        
        /// <summary>
        /// Load mock friends list for Editor testing
        /// </summary>
        private void LoadMockFriendsList()
        {
            string userName = _telegramDebugData?.UserName ?? "TestUser";
            
            _friendsData = new FriendsListResponse
            {
                friends = new List<FriendInfo>
                {
                    new FriendInfo
                    {
                        playerId = "1",
                        nickname = $"{userName}_Friend1",
                        level = 5,
                        totalEarnings = 10000,
                        yourBonus = 5000,
                        invitedAt = DateTime.Now.AddDays(-7).ToString("o")
                    },
                    new FriendInfo
                    {
                        playerId = "2",
                        nickname = $"{userName}_Friend2",
                        level = 3,
                        totalEarnings = 5000,
                        yourBonus = 5000,
                        invitedAt = DateTime.Now.AddDays(-3).ToString("o")
                    },
                    new FriendInfo
                    {
                        playerId = "3",
                        nickname = $"{userName}_Friend3",
                        level = 1,
                        totalEarnings = 1000,
                        yourBonus = 5000,
                        invitedAt = DateTime.Now.AddDays(-1).ToString("o")
                    }
                },
                totalFriends = 3,
                totalBonusEarned = 15000
            };
            
            Debug.Log($"<color=#FFFF00>[ReferralService] Loaded MOCK friends list: {_friendsData.totalFriends} friends</color>");
            OnFriendsListLoaded?.Invoke(_friendsData);
        }
#endif
        
        #endregion
    }
}
