namespace LumStoreAPI.Core.Interfaces.Sytems
{
    public interface ICacheService
    {
        T? GetCache<T>(Func<T> func, Action<ICacheBuilder>? cacheBuider = null);
        Task<T?> GetCacheAsync<T>(Func<Task<T>> func, Action<ICacheBuilder>? cacheBuider = null);
        void TouchKey(string key);
    }
}
