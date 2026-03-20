namespace WattsTap.Constants
{
    public static class SharedDataConstants
    {
        public const string TelegramUser = "TelegramUser";
        public const string TelegramChatId = "TelegramChatId";
        
        // Referral API data (legacy)
        public const string ReferralAuthToken = "ReferralAuthToken";
        public const string ReferralCode = "ReferralCode";
        public const string ReferralData = "ReferralData";
        public const string FriendsData = "FriendsData";
        
        // Core Server data (new)
        public const string CoreAuthToken = "CoreAuthToken";
        public const string CoreTokenExpiresAt = "CoreTokenExpiresAt";
        public const string InitData = "InitData";
        public const string UserProfile = "UserProfile";
        public const string PlayerState = "PlayerState";
        public const string PlayerEnergy = "PlayerEnergy";
    }
}