using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.DocumentPages;
using LumStoreAPI.Core.Interfaces.Sytems;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IPageRetrieveContext
    {
        Task<IEnumerable<T>> GetPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null, Action<ICacheBuilder>? cacheBuilder = null) where T : DocumentPage;
        Task<IPagedEnumerable<T>> GetPagedPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null, Action<ICacheBuilder>? cacheBuilder = null) where T : DocumentPage;
    }
}
