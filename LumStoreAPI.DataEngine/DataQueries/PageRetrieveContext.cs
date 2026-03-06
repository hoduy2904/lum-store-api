using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.DocumentPages;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.DataEngine.TreeNodeContentEngine;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LumStoreAPI.Infrastructure.Extensions;

namespace LumStoreAPI.Infrastructure.Presentation
{
    public class PageRetrieveContext : IPageRetrieveContext
    {
        private readonly LumStoreContext _lumStoreContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public PageRetrieveContext(LumStoreContext lumStoreContext, IHttpContextAccessor httpContextAccessor)
        {
            _lumStoreContext = lumStoreContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IEnumerable<T>> GetPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null) where T : DocumentPage
        {

            var treeContent = GetPages(where);

            return await treeContent.ToListAsync();
        }

        public async Task<IPagedEnumerable<T>> GetPagedPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null) where T : DocumentPage
        {
            var treeContent = GetPages(where);

            var data = await treeContent.ToListAsync();

            int totalRecords = await treeContent.AsQueryable().CountAsync();

            return data.AsPagedEnumerable(totalRecords);
        }

        private ITreeNodeContent<T> GetPages<T>(Action<ITreeNodeContent<T>>? where = null) where T : DocumentPage
        {

            var data = _lumStoreContext.Set<T>()
                  .Include(x => x.Node)
                  .AsNoTracking();

            ITreeNodeContent<T> treeContent = new TreeNodeContent<T>(_lumStoreContext, data, _httpContextAccessor);

            where?.Invoke(treeContent);
            return treeContent;
        }
    }
}
