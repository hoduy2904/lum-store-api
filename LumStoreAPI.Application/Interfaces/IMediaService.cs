using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IMediaService
    {
        Task<MediaItem?> GetMediaItemAsync(Guid fileID);
        Task<IEnumerable<MediaItem>> InsertMediaItemAsync(MediaItemInsertRequest request);
        Task<MediaItem> UpdateMediaItemAsync(MediaItemUpdateRequest request);
    }
}
