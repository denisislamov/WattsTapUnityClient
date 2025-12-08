using UnityEngine;

namespace WattsTap.Core.Configs.Referral
{
    [CreateAssetMenu(fileName = "ReferralConfig", menuName = "WattsTap/Configs/Referral/ReferralConfig")]
    public class ReferralConfig : BaseConfig
    {
        [Header("API Settings")]
        [Tooltip("Base URL for the referral API")]
        public string BaseUrl = "http://localhost:8000";
        
        [Tooltip("Request timeout in seconds")]
        public int RequestTimeout = 30;
        
        [Header("Bot Settings")]
        [Tooltip("Telegram bot username for invite links")]
        public string BotUsername = "wattstap_eu_bot";
        
        [Tooltip("Mini App short name (set in BotFather)")]
        public string MiniAppShortName = "app";
        
        [Header("Default Values")]
        [Tooltip("Default bonus per friend if not provided by server")]
        public int DefaultBonusPerFriend = 5000;
        
        [Header("Share Settings")]
        [Tooltip("Text to share when inviting friends")]
        public string ShareText = "Join me in WattsTap and get bonus coins! ⚡🎮";
        
        /// <summary>
        /// Constructs the full invite link from a referral code.
        /// Uses startapp parameter for Mini Apps (not start which is for bot commands).
        /// Format: https://t.me/BotUsername/AppShortName?startapp=REF_CODE
        /// </summary>
        public string GetInviteLink(string referralCode)
        {
            // Use startapp for Mini Apps, not start
            // start= sends parameter to bot via /start command
            // startapp= sends parameter to Mini App via initDataUnsafe.start_param
            return $"https://t.me/{BotUsername}/{MiniAppShortName}?startapp=REF_{referralCode}";
        }
    }
}

