namespace WattsTap.Core
{
    public interface ISharedData<T>
    {
        T Data { get; set; }
        void SetData(T data);
        T GetData();
    }
}