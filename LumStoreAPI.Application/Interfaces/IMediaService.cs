using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IMediaService
    {
        Task<IPagedEnumerable<MediaItemDTO>> GetMediaItemsAsync(int categoryId, int page, int pageSize, string? q = "");
        Task<IPagedEnumerable<MediaItemDTO>> GetMediaItemsAsync(Guid[] fileIds);
        Task<IEnumerable<MediaItemDTO>> InsertMediaItemAsync(MediaItemInsertRequest request);
        Task<MediaItemDTO?> GetMediaItemAsync(Guid fileID);
        Task<MediaItemDTO?> UpdateMediaItemAsync(MediaItemUpdateRequest request);
        Task<MediaFolderDTO> CreateMediaFolderAsync(MediaFolderRequest mediaFolder);
        Task<int> RenameMediaFolderAsync(int categoryId, MediaFolderRequest mediaFolder);
        Task<int> DeleteFile(Guid fileID);
        Task<int> DeleteFiles(Guid[] fileIDs);
        Task<int> DeleteFolder(int folderID);
        Task<int> DeleteFolders(int[] folderIds);
    }
}
