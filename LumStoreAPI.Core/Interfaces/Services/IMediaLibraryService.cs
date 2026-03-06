using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Core.Interfaces.Services
{
    public interface IMediaLibraryService
    {
        Task<MediaItem?> GetMediaItemAsync(Guid fileID);
        Task<IEnumerable<MediaItem>> GetMediaItemsAsync(Guid[] fileIDs);
        Task<IPagedEnumerable<MediaItem>> GetMediaItemsAsync(int categoryId, int page, int pageSize, string? q = "");
        Task<MediaItem> InsertMediaItemAsync(MediaItem mediaItem);
        Task<IEnumerable<MediaItem>> InsertMediaItemsAsync(IEnumerable<MediaItem> mediaItems);
    }
}
