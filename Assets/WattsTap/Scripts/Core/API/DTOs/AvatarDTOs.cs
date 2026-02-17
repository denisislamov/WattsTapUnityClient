using System;

namespace WattsTap.Core.API
{
    #region Avatar DTOs
    
    /// <summary>
    /// Request to purchase an avatar with currency
    /// </summary>
    [Serializable]
    public class PurchaseAvatarRequest
    {
        public string avatarId;
        public int price;
        public string currency;
    }
    
    /// <summary>
    /// Response after avatar purchase attempt
    /// </summary>
    [Serializable]
    public class PurchaseAvatarResponse
    {
        public bool success;
        public string avatarId;
        public long newWattsBalance;
        public string[] unlockedAvatars;
        public string message;
    }
    
    /// <summary>
    /// Request to unlock an avatar by level (free)
    /// </summary>
    [Serializable]
    public class UnlockAvatarByLevelRequest
    {
        public string avatarId;
    }
    
    /// <summary>
    /// Response after level-based avatar unlock
    /// </summary>
    [Serializable]
    public class UnlockAvatarByLevelResponse
    {
        public bool success;
        public string avatarId;
        public string[] unlockedAvatars;
        public string message;
    }
    
    /// <summary>
    /// Response with player's avatar data
    /// </summary>
    [Serializable]
    public class GetAvatarsResponse
    {
        public bool success;
        public string[] unlockedAvatars;
        public string currentAvatar;
    }
    
    #endregion
}
