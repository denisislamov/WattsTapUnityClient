namespace WattsTap.Core.GameLoop
{
    public interface IUpdateService : IService
    {
        public void Register(IUpdatable updatable);
        public void Unregister(IUpdatable updatable);
    }
}