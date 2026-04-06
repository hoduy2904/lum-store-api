using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.DocumentPages;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.DataEngine.TreeNodeContentEngine;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Presentation
{
    public class PageRetrieveContext : IPageRetrieveContext
    {
        private readonly ICacheService _cacheService;
        private readonly LumStoreContext _lumStoreContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PageRetrieveContext(LumStoreContext lumStoreContext, IHttpContextAccessor httpContextAccessor, ICacheService cacheService)
        {
            _lumStoreContext = lumStoreContext;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<T>> GetPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null, Action<ICacheBuilder>? cacheBuilder = null) where T : DocumentPage
        {
            return (await _cacheService.GetCacheAsync<IEnumerable<T>>(async () =>
            {
                var treeContent = GetPages(where);
                return await treeContent.ToListAsync();
            }, cacheBuilder)) ?? Enumerable.Empty<T>();
        }

        public async Task<IPagedEnumerable<T>> GetPagedPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null, Action<ICacheBuilder>? cacheBuilder = null) where T : DocumentPage
        {
            var cacheData = await _cacheService.GetCacheAsync(async () =>
             {
                 var treeContent = GetPages(where);

                 int totalRecords = await treeContent.AsQueryable().CountAsync();

                 var data = await treeContent.ToListAsync();

                 return data.AsPagedEnumerable(totalRecords);
             }, cacheBuilder);

            return cacheData ?? Enumerable.Empty<T>().AsPagedEnumerable(0);
        }

        private ITreeNodeContent<T> GetPages<T>(Action<ITreeNodeContent<T>>? where = null) where T : DocumentPage
        {

            var data = _lumStoreContext.Set<T>()
            .Include(x => x.Node)
            .AsNoTrackingWithIdentityResolution();

            ITreeNodeContent<T> treeContent = new TreeNodeContent<T>(_lumStoreContext, data, _httpContextAccessor);

            where?.Invoke(treeContent);
            return treeContent;
        }
    }
}
