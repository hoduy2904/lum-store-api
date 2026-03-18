using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IMediaService
    {
        Task<IPagedEnumerable<MediaItemDTO>> GetMediaItemsAsync(MediaItemListingRequest request);
        Task<IPagedEnumerable<MediaFolderDTO>> GetFoldersAsync(int page, int pageSize, string? search = null);
        Task<IEnumerable<MediaItemDTO>> GetMediaItemsAsync(Guid[] fileIds);
        Task<IEnumerable<MediaItemDTO>> InsertMediaItemAsync(MediaItemInsertRequest request);
        Task<MediaItemDTO?> GetMediaItemAsync(Guid fileID);
        Task<MediaItemDTO?> UpdateMediaItemAsync(MediaItemUpdateRequest request);
        Task<MediaFolderDTO> CreateMediaFolderAsync(MediaFolderRequest mediaFolder);
        Task<int> RenameMediaFolderAsync(int categoryId, MediaFolderRequest mediaFolder);
        Task<int> DeleteFileAsync(Guid fileID);
        Task<int> DeleteFilesAsync(Guid[] fileIDs);
        Task<int> DeleteFolderAsync(int folderID);
        Task<int> DeleteFoldersAsync(int[] folderIds);
    }
}
