namespace WattsTap.Core
{
    public class SharedData<T> : ISharedData<T>
    {
        public T Data { get; set; }

        public SharedData(T data)
        {
            Data = data;
        }

        public void SetData(T data)
        {
            Data = data;
        }

        public T GetData()
        {
            return Data;
        }
    }
}