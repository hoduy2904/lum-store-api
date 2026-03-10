using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Infrastructure.Models.Riches;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        private IChangeToken GetToken(string key)
        {
            var cts = _tokens.GetOrAdd(key, _ => new CancellationTokenSource());
            return new CancellationChangeToken(cts.Token);
        }
        public T? GetCache<T>(Func<T> func, Action<ICacheBuilder>? cacheBuider = null)
        {
            if (cacheBuider == null)
                return func();
            var cacheBuilderPr = new CacheBuilder();
            cacheBuider.Invoke(cacheBuilderPr);

            if (string.IsNullOrWhiteSpace(cacheBuilderPr.CacheSetting.CacheKey))
            {
                throw new InvalidDataException("Cache Key cannot NULL or empty");
            }

            if (_memoryCache.TryGetValue<T>(cacheBuilderPr.CacheSetting.CacheKey, out T? item))
            {
                return item;
            }

            item = func();

            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheBuilderPr.CacheSetting.CacheMinutes));

            foreach (var dependency in cacheBuilderPr.CacheDependency.GetDependencies())
            {
                options.AddExpirationToken(this.GetToken(dependency));
            }
            _memoryCache.Set(cacheBuilderPr.CacheSetting.CacheKey, item, options);

            return item;
        }

        public async Task<T?> GetCacheAsync<T>(Func<Task<T>> func, Action<ICacheBuilder>? cacheBuider = null)
        {
            if (cacheBuider == null)
                return await func();
            var cacheBuilderPr = new CacheBuilder();
            cacheBuider.Invoke(cacheBuilderPr);

            if (string.IsNullOrWhiteSpace(cacheBuilderPr.CacheSetting.CacheKey))
            {
                throw new InvalidDataException("Cache Key cannot NULL or empty");
            }

            if (_memoryCache.TryGetValue<T>(cacheBuilderPr.CacheSetting.CacheKey, out T? item))
            {
                return item;
            }

            item = await func();

            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheBuilderPr.CacheSetting.CacheMinutes));

            foreach (var dependency in cacheBuilderPr.CacheDependency.GetDependencies())
            {
                options.AddExpirationToken(this.GetToken(dependency));
            }
            _memoryCache.Set(cacheBuilderPr.CacheSetting.CacheKey, item, options);

            return item;
        }

        public void TouchKey(string key)
        {
            if (_tokens.TryRemove(key, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }
    }
}
