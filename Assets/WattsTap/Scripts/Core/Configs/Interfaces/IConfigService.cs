namespace WattsTap.Core.Configs
{
    public interface IConfigService : IService
    {
        T GetConfig<T>(string key = "default") where T : BaseConfig;
        void RegisterConfig<T>(string key, T config) where T : BaseConfig;
    }
}