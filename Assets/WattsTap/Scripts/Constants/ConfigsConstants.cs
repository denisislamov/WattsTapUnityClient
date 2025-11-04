namespace WattsTap.Constants
{
    public static class ConfigsConstants
    {
        public const string UIConfig = "UIConfig";
#if !RELEASE
        public const string TelegramDebugData = "HNTelegramDebugDataDev";
#else
        public const string TelegramDebugData = "HNTelegramDebugDataRelease";
#endif
    }
}