using UnityEngine;
using WattsTap.Core.Configs;

namespace WattsTap.Core.Configs.CoreServer
{
    /// <summary>
    /// Configuration for the new Core API server (api-dev.wattstap.energy).
    /// All endpoints and connection settings are defined here.
    /// </summary>
    [CreateAssetMenu(fileName = "CoreServerConfig", menuName = "WattsTap/Configs/CoreServer/CoreServerConfig")]
    public class CoreServerConfig : BaseConfig
    {
        [Header("Server")]
        [Tooltip("Base URL for the Core API server")]
        public string BaseUrl = "https://api-dev.wattstap.energy";

        [Tooltip("Request timeout in seconds")]
        public int RequestTimeout = 30;

        [Header("Token Refresh")]
        [Tooltip("How many seconds before token expiry to trigger refresh")]
        public int TokenRefreshBufferSeconds = 60;

        [Tooltip("Enable automatic token refresh")]
        public bool EnableAutoRefresh = true;

        [Tooltip("Interval to check if token needs refresh (seconds)")]
        public float RefreshCheckInterval = 30f;

        [Header("Tap Batching")]
        [Tooltip("Interval between tap batch sends (seconds)")]
        [Range(1f, 30f)]
        public float TapSyncInterval = 3f;

        [Header("Debug")]
        [Tooltip("Enable verbose debug logging")]
        public bool DebugLogging = true;

        [Header("Endpoints — Auth")]
        public string AuthTelegram = "/auth/telegram";
        public string AuthRefresh = "/auth/refresh";
        public string AuthLogout = "/auth/logout";

        [Header("Endpoints — User")]
        public string UserMe = "/user/me";
        public string UserLanguage = "/user/language";
        public string UserEventsRead = "/user/events/read";

        [Header("Endpoints — Progress")]
        public string Progress = "/progress";
        public string ProgressReset = "/progress/reset";

        [Header("Endpoints — Balance")]
        public string BalanceMining = "/balance/mining";

        [Header("Endpoints — Social")]
        public string SocialMyReferral = "/social/my-referral";
        public string SocialFriends = "/social/friends";
        public string SocialBonusClaim = "/social/bonus/claim";

        [Header("Endpoints — Referral")]
        public string ReferralApply = "/referral/apply";

        [Header("Endpoints — Avatars")]
        public string Avatars = "/avatars";
        public string AvatarsPurchase = "/avatars/purchase";
        /// <summary>Use string.Format(AvatarsClaimFmt, avatarId)</summary>
        public string AvatarsClaimFmt = "/avatars/{0}/claim";

        [Header("Endpoints — Wallet")]
        public string WalletMyWallet = "/wallet/my-wallet";
        public string WalletPayload = "/wallet/payload";
        public string WalletValidate = "/wallet/validate";

        [Header("Endpoints — Game / Inventory")]
        public string GameItemsCatalog = "/game/items/catalog";
        public string GameInventory = "/game/inventory";
        public string GameInventoryEquip = "/game/inventory/equip";
        public string GameInventoryUnequip = "/game/inventory/unequip";
        public string GameInventoryUpgrade = "/game/inventory/upgrade";

        [Header("Endpoints — Orders")]
        /// <summary>Use string.Format(OrderByIdFmt, orderId)</summary>
        public string OrderByIdFmt = "/orders/{0}";

        // Helper methods
        public string GetFullUrl(string endpoint) => BaseUrl + endpoint;
        public string GetAvatarClaimUrl(string avatarId) => BaseUrl + string.Format(AvatarsClaimFmt, avatarId);
        public string GetOrderUrl(string orderId) => BaseUrl + string.Format(OrderByIdFmt, orderId);
    }
}

