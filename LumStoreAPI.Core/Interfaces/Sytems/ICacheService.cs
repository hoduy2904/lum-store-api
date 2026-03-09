namespace LumStoreAPI.Core.Interfaces.Sytems
{
    public interface ICacheService
    {
        T? GetCache<T>(Func<T> func, Action<ICacheBuilder> cacheBuider);
        Task<T?> GetCacheAsync<T>(Func<Task<T>> func, Action<ICacheBuilder> cacheBuider);
        void TouchKey(string key);
    }
}
