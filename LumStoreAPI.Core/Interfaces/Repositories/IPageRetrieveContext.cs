using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.DocumentPages;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IPageRetrieveContext
    {
        Task<IEnumerable<T>> GetPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null) where T : DocumentPage;
        Task<IPagedEnumerable<T>> GetPagedPagesAsync<T>(Action<ITreeNodeContent<T>>? where = null) where T : DocumentPage;
    }
}
