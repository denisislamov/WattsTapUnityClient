using System;
using System.Collections;

namespace WattsTap.Core.API
{
    /// <summary>
    /// Service for communicating with the new Core API server (api-dev.wattstap.energy).
    /// Handles authentication, token refresh, progress (tap-based), avatars, social, user profile.
    /// </summary>
    public interface ICoreServerService : IService
    {
        #region Auth State

        /// <summary>JWT auth token</summary>
        string AuthToken { get; }

        /// <summary>Whether the user is authenticated</summary>
        bool IsAuthenticated { get; }

        /// <summary>When the current token expires</summary>
        DateTime TokenExpiresAt { get; }

        /// <summary>Whether the token needs to be refreshed soon</summary>
        bool NeedsTokenRefresh { get; }

        /// <summary>Current player energy from last server response</summary>
        int CurrentEnergy { get; }

        #endregion

        #region Auth

        /// <summary>Authenticate via Telegram initData</summary>
        IEnumerator Authenticate(string initData, string referralCode,
            Action<AuthResponse> onSuccess, Action<string> onError);

        /// <summary>Refresh the JWT token (uses cookie-based refresh or re-auth fallback)</summary>
        IEnumerator RefreshToken(Action<AuthResponse> onSuccess, Action<string> onError);

        /// <summary>Logout and invalidate tokens</summary>
        IEnumerator Logout(Action<LogoutResponse> onSuccess, Action<string> onError);

        /// <summary>Start automatic token refresh coroutine</summary>
        void StartAutoRefresh(UnityEngine.MonoBehaviour coroutineRunner);

        /// <summary>Stop automatic token refresh</summary>
        void StopAutoRefresh();

        #endregion

        #region User

        /// <summary>Get current user profile and events</summary>
        IEnumerator GetUserMe(Action<UserMeResponse> onSuccess, Action<string> onError);

        /// <summary>Update user language</summary>
        IEnumerator UpdateLanguage(string langCode,
            Action<UpdateLanguageResponse> onSuccess, Action<string> onError);

        /// <summary>Mark events as read</summary>
        IEnumerator MarkEventsRead(MarkEventsReadRequest request,
            Action<MarkEventsReadResponse> onSuccess, Action<string> onError);

        #endregion

        #region Progress (Tap-based)

        /// <summary>Load player progress from server</summary>
        IEnumerator LoadProgress(Action<ExtendedLoadProgressResponse> onSuccess, Action<string> onError);

        /// <summary>Send taps to server (new tap-based model)</summary>
        IEnumerator SendTaps(int tapCount,
            Action<TapProgressResponse> onSuccess, Action<string> onError);

        /// <summary>Reset player progress</summary>
        IEnumerator ResetProgress(Action<ResetProgressResponse> onSuccess, Action<string> onError);

        #endregion

        #region Mining Balance

        /// <summary>Load mining balance config (public, no auth needed)</summary>
        IEnumerator LoadMiningBalance(Action<MiningBalancePublicResponse> onSuccess, Action<string> onError);

        #endregion

        #region Avatars

        /// <summary>Get player's avatars including full catalog</summary>
        IEnumerator GetAvatars(Action<GetAvatarsResponse> onSuccess, Action<string> onError);

        /// <summary>Purchase an avatar with currency</summary>
        IEnumerator PurchaseAvatar(PurchaseAvatarRequest request,
            Action<PurchaseAvatarResponse> onSuccess, Action<string> onError);

        /// <summary>Claim a free avatar (replaces unlock-by-level)</summary>
        IEnumerator ClaimAvatar(string avatarId,
            Action<ClaimAvatarResponse> onSuccess, Action<string> onError);

        #endregion

        #region Social / Referral

        /// <summary>Get user's referral information</summary>
        IEnumerator GetMyReferral(Action<MyReferralResponse> onSuccess, Action<string> onError);

        /// <summary>Get user's friends list</summary>
        IEnumerator GetFriends(Action<FriendsListResponse> onSuccess, Action<string> onError);

        /// <summary>Claim referral bonus</summary>
        IEnumerator ClaimReferralBonus(Action<ClaimBonusResponse> onSuccess, Action<string> onError);

        /// <summary>Apply a referral code manually</summary>
        IEnumerator ApplyReferralCode(string referralCode, string source,
            Action<ReferralApplyResponse> onSuccess, Action<string> onError);

        #endregion

        #region Game Items / Inventory

        /// <summary>Get the full item catalog from server</summary>
        IEnumerator GetCatalog(Action<CatalogResponse> onSuccess, Action<string> onError);

        /// <summary>Get player's inventory, equipment, and currencies</summary>
        IEnumerator GetInventory(Action<InventoryResponse> onSuccess, Action<string> onError);

        /// <summary>Equip an item by player-item instance id</summary>
        IEnumerator EquipItem(string playerItemId,
            Action<EquipItemResponse> onSuccess, Action<string> onError);

        /// <summary>Unequip a slot</summary>
        IEnumerator UnequipItem(string slot,
            Action<UnequipItemResponse> onSuccess, Action<string> onError);

        /// <summary>Upgrade an item</summary>
        IEnumerator UpgradeItem(string playerItemId,
            Action<UpgradeItemResponse> onSuccess, Action<string> onError);

        #endregion
    }
}

