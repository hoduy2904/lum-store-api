using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Infrastructure.Models.Riches
{
    internal class CacheBuilder : ICacheBuilder
    {
        public CacheSetting CacheSetting { get; } = new();
        public CacheDependency CacheDependency { get; } = new();
        public ICacheBuilder Dependencies(Action<CacheDependency> action)
        {
            action.Invoke(CacheDependency);
            return this;
        }

        public ICacheBuilder Expiration(int cacheMinutes)
        {
            CacheSetting.CacheMinutes = cacheMinutes;
            return this;
        }

        public ICacheBuilder Key(string key)
        {
            CacheSetting.CacheKey = key;
            return this;
        }
    }
}
