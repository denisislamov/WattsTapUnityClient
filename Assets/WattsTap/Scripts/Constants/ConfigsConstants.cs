namespace WattsTap.Constants
{
    public static class ConfigsConstants
    {
        public const string UIConfig = "UIConfig";
        public const string ReferralConfig = "ReferralConfig";
#if !RELEASE
        public const string TelegramDebugData = "TelegramDebugDataDev";
#else
        public const string TelegramDebugData = "TelegramDebugDataRelease";
#endif
    }
}