using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IMediaLibraryRepository
    {
        Task<MediaLibrary?> GetMediaItemAsync(Guid fileID);
        Task<IEnumerable<MediaLibrary>> GetMediaItemsAsync(Expression<Func<MediaLibrary, bool>>? where = null);
        Task<IEnumerable<string>> GetMediaDirectFilePaths(Guid[] fileIds);
        Task<IEnumerable<string>> GetMediaDirectFilePaths(Expression<Func<MediaLibrary, bool>>? where = null);
        Task<IPagedEnumerable<MediaLibrary>> GetMediaItemsAsync(int page, int pageSize, Expression<Func<MediaLibrary, bool>>? where = null);
        Task<MediaLibrary> InsertMediaItem(MediaLibrary mediaLibrary);
        Task<IEnumerable<MediaLibrary>> InsertMediaItems(IEnumerable<MediaLibrary> mediaLibraries);
        Task<MediaLibrary> UpdateMediaItem(MediaLibrary mediaLibrary);
        Task<int> DeleteMediaItem(Guid fileID);
        Task<int> DeleteMediaItems(Expression<Func<MediaLibrary, bool>> where);
    }
}
