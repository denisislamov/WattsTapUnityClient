using System;
using System.Collections.Generic;

namespace WattsTap.Core.API
{
    #region Request DTOs
    
    /// <summary>
    /// Request for Telegram authentication
    /// </summary>
    [Serializable]
    public class TelegramAuthRequest
    {
        public string initData;
        public string referralCode;
    }
    
    #endregion
    
    #region Response DTOs
    
    /// <summary>
    /// Information about the user who sent the referral
    /// </summary>
    [Serializable]
    public class ReferrerInfo
    {
        public long userId;
        public string nickname;
        public string avatarUrl;
        public int level;
    }
    
    /// <summary>
    /// Result of referral code application
    /// </summary>
    [Serializable]
    public class ReferralResult
    {
        public bool applied;
        public ReferrerInfo referrer;
        public int bonusForReferrer;
        public string message;
    }
    
    /// <summary>
    /// Basic player information returned after authentication
    /// </summary>
    [Serializable]
    public class PlayerInfo
    {
        public string playerId;
        public string nickname;
        public int level;
        public bool isNewPlayer;
        public string referralCode;
    }
    
    /// <summary>
    /// Response from authentication endpoint
    /// </summary>
    [Serializable]
    public class AuthResponse
    {
        public string token;
        public int expiresIn;
        public PlayerInfo player;
        public ReferralResult referral;
    }
    
    /// <summary>
    /// Information about a friend
    /// </summary>
    [Serializable]
    public class FriendInfo
    {
        public string playerId;
        public string nickname;
        public int level;
        public string avatarUrl;
        public long totalEarnings;
        public long yourBonus;
        public string invitedAt;
    }
    
    /// <summary>
    /// Response containing list of friends
    /// </summary>
    [Serializable]
    public class FriendsListResponse
    {
        public List<FriendInfo> friends;
        public int totalFriends;
        public long totalBonusEarned;
        /// <summary>NEW: total pending bonus from friends</summary>
        public long totalBonusPending;
    }
    
    /// <summary>
    /// Response containing user's referral information
    /// </summary>
    [Serializable]
    public class MyReferralResponse
    {
        public string referralCode;
        public string inviteLink;
        public int bonusPerFriend;
        public int totalFriendsInvited;
        public long totalBonusEarned;
        /// <summary>NEW: pending bonus available from referrals</summary>
        public long pendingBonus;
        /// <summary>NEW: whether bonus can be claimed</summary>
        public bool availableToClaim;
    }
    
    #endregion
}





