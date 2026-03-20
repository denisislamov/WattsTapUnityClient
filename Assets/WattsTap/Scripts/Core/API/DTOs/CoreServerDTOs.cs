using System;
using System.Collections.Generic;

namespace WattsTap.Core.API
{
    #region User Profile DTOs

    /// <summary>
    /// User profile from GET /user/me and POST /user/language.
    /// </summary>
    [Serializable]
    public class UserProfile
    {
        public string id;
        public string username;
        public string firstName;
        public string lastName;
        public string telegramId;
        public string photoUrl;
        public string languageCode;
        public string currentAvatarId;
        public string createdAt;
    }

    /// <summary>
    /// User event (notifications, rewards, etc.).
    /// </summary>
    [Serializable]
    public class UserEvent
    {
        public string id;
        public string type;
        public string title;
        public string message;
        public bool isRead;
        public string createdAt;
    }

    /// <summary>
    /// Response from GET /user/me.
    /// </summary>
    [Serializable]
    public class UserMeResponse
    {
        public UserProfile user;
        public List<UserEvent> events;
        public int unreadEventsCount;
    }

    /// <summary>
    /// Request for POST /user/language.
    /// </summary>
    [Serializable]
    public class UpdateLanguageRequest
    {
        public string newLangCode;
    }

    /// <summary>
    /// Response from POST /user/language.
    /// </summary>
    [Serializable]
    public class UpdateLanguageResponse
    {
        public UserProfile user;
    }

    /// <summary>
    /// Request for POST /user/events/read.
    /// </summary>
    [Serializable]
    public class MarkEventsReadRequest
    {
        public bool readAll;
        public string[] eventIds;
    }

    /// <summary>
    /// Response from POST /user/events/read.
    /// </summary>
    [Serializable]
    public class MarkEventsReadResponse
    {
        public bool success;
        public int updatedCount;
        public int unreadEventsCount;
    }

    #endregion

    #region Auth Extension DTOs

    /// <summary>
    /// Response from POST /auth/logout.
    /// </summary>
    [Serializable]
    public class LogoutResponse
    {
        public string message;
    }

    #endregion

    #region Social Extension DTOs

    /// <summary>
    /// Response from POST /social/bonus/claim.
    /// </summary>
    [Serializable]
    public class ClaimBonusResponse
    {
        public bool success;
        public long claimedAmount;
        public int claimedFriendsCount;
        public long newWattsBalance;
        public long totalBonusEarned;
        public long totalBonusPending;
    }

    /// <summary>
    /// Request for POST /referral/apply.
    /// </summary>
    [Serializable]
    public class ReferralApplyRequest
    {
        public string referralCode;
        public string source;
    }

    /// <summary>
    /// Response from POST /referral/apply.
    /// </summary>
    [Serializable]
    public class ReferralApplyResponse
    {
        public string invitedBy;
        public string userReferralCode;
        public bool activeStatus;
    }

    #endregion

    #region Avatar Extension DTOs

    /// <summary>
    /// Avatar catalog item from GET /avatars response.
    /// </summary>
    [Serializable]
    public class AvatarCatalogItem
    {
        public string id;
        public string code;
        public string title;
        public string imageUrl;
        public string previewUrl;
        public bool isActive;
        /// <summary>"FREE", "COINS", "LEVEL"</summary>
        public string unlockType;
        public int unlockLevel;
        /// <summary>Price as string: "0", "10". Use int.Parse() when needed.</summary>
        public string price;
        public bool isOwned;
        public bool isCurrent;
        public bool canPurchase;
    }

    /// <summary>
    /// Response from POST /avatars/{id}/claim.
    /// </summary>
    [Serializable]
    public class ClaimAvatarResponse
    {
        public string avatarId;
        public string currentAvatarId;
    }

    #endregion
}

