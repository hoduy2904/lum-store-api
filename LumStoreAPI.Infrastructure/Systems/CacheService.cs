using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using LumStoreAPI.Infrastructure.Models.Riches;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();

        public CacheService(IMemoryCache memoryCache, IServiceProvider serviceProvider)
        {
            _memoryCache = memoryCache;
            _serviceProvider = serviceProvider;
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

            var childrenPathReg = new Regex(@"node\|\d+\|children");

            var childrenParents = new CacheDependency();
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
            foreach (var dependency in cacheBuilderPr.CacheDependency.GetDependencies())
            {
                options.AddExpirationToken(this.GetToken(dependency));
                if (childrenPathReg.IsMatch(dependency))
                {
                    var value = dependency.Split('|')[1];
                    if (int.TryParse(value, out int nodeId))
                    {
                        var childrenNodeIds = context.DocumentLinkedNodes.Where(x => x.Ancestor == nodeId)
                          .Select(x => x.Descendant)
                          .ToArray();

                        foreach (var childrenNodeId in childrenNodeIds)
                        {
                            childrenParents.NodeID(childrenNodeId);
                        }
                    }
                }
            }

            foreach (var nodeIdCache in childrenParents.GetDependencies())
            {
                options.AddExpirationToken(this.GetToken(nodeIdCache));
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

            var options = new MemoryCacheEntryOptions();
            if (cacheBuilderPr.CacheSetting.CacheMinutes > 0)
            {
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheBuilderPr.CacheSetting.CacheMinutes));
            }
            var childrenPathReg = new Regex(@"node\|\d+\|children");

            var childrenParents = new CacheDependency();
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
            foreach (var dependency in cacheBuilderPr.CacheDependency.GetDependencies())
            {
                options.AddExpirationToken(this.GetToken(dependency));
                if (childrenPathReg.IsMatch(dependency))
                {
                    var value = dependency.Split('|')[1];
                    if (int.TryParse(value, out int nodeId))
                    {
                        var childrenNodeIds = await context.DocumentLinkedNodes.Where(x => x.Ancestor == nodeId)
                          .Select(x => x.Descendant)
                          .ToArrayAsync();

                        foreach (var childrenNodeId in childrenNodeIds)
                        {
                            childrenParents.NodeID(childrenNodeId);
                        }
                    }
                }
            }

            foreach (var nodeIdCache in childrenParents.GetDependencies())
            {
                options.AddExpirationToken(this.GetToken(nodeIdCache));
            }
            _memoryCache.Set(cacheBuilderPr.CacheSetting.CacheKey, item, options);

            return item;
        }

        public void TouchKey(params string[] keys)
        {
            foreach (var key in keys)
            {
                if (_tokens.TryRemove(key, out var cts))
                {
                    cts.Cancel();
                    cts.Dispose();
                }
            }
        }
    }
}
