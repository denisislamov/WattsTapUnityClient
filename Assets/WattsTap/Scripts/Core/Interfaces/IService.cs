namespace WattsTap.Core
{
    public interface IService
    {
        int InitializationOrder { get; }
        void Initialize();
        void Shutdown();
        bool IsInitialized { get; }
    }
}