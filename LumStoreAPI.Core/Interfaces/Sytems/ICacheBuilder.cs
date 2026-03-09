using LumStoreAPI.Core.Models.Riches;

namespace LumStoreAPI.Core.Interfaces.Sytems
{
    public interface ICacheBuilder
    {
        ICacheBuilder Key(string key);
        ICacheBuilder Dependencies(Action<CacheDependency> action);
        ICacheBuilder Expiration(int cacheMinutes);
    }
}
