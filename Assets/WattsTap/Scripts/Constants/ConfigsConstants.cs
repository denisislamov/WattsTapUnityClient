namespace WattsTap.Constants
{
    public static class ConfigsConstants
    {
        public const string UIConfig = "UIConfig";
#if !RELEASE
        public const string TelegramDebugData = "TelegramDebugDataDev";
#else
        public const string TelegramDebugData = "TelegramDebugDataRelease";
#endif
    }
}