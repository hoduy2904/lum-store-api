using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.Services
{
    internal class MediaService : IMediaService
    {
        private readonly IMediaLibraryCategoryRepository _mediaLibraryCategoryRepository;
        private readonly IMediaLibraryRepository _mediaLibraryRepository;

        public MediaService(IMediaLibraryCategoryRepository mediaLibraryCategoryRepository, IMediaLibraryRepository mediaLibraryRepository)
        {
            _mediaLibraryCategoryRepository = mediaLibraryCategoryRepository;
            _mediaLibraryRepository = mediaLibraryRepository;
        }

        public async Task<MediaFolderDTO> CreateMediaFolderAsync(MediaFolderRequest mediaFolder)
        {
            var folder = await _mediaLibraryCategoryRepository.InsertCategory(new MediaLibraryCategory
            {
                CategoryName = mediaFolder.FolderName,
                FolderName = mediaFolder.FolderName.Slug
            });

            return new MediaFolderDTO(folder);
        }

        public Task<int> DeleteFileAsync(Guid fileID)
        {
            return this.DeleteFilesAsync([fileID]);
        }

        public Task<int> DeleteFilesAsync(Guid[] fileIDs)
        {
            if (fileIDs.Any())
                return _mediaLibraryRepository.DeleteMediaItems(x => fileIDs.Contains(x.FileID));
            return Task.FromResult(0);
        }

        public Task<int> DeleteFolderAsync(int folderID)
        {
            return DeleteFoldersAsync([folderID]);
        }

        public async Task<int> DeleteFoldersAsync(int[] folderIds)
        {
            var mediaDirectPaths = await _mediaLibraryCategoryRepository.GetMediaLibraryFolderPaths(folderIds);

            var result = await _mediaLibraryCategoryRepository.DeleteCategories(x => folderIds.Contains(x.CategoryID));
            if (result > 0)
            {
                Parallel.ForEach(mediaDirectPaths, path =>
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                });
            }
            return result;
        }

        public async Task<IPagedEnumerable<MediaFolderDTO>> GetFoldersAsync(int page, int pageSize, string? search = null)
        {
            var folders = await _mediaLibraryCategoryRepository.GetMediaLibraryCategoriesAsync(page, pageSize,

                x => string.IsNullOrEmpty(search) || x.CategoryName.Contains(search));

            var data = folders.Select(x => new MediaFolderDTO(x));

            return data;
        }

        public async Task<MediaItemDTO?> GetMediaItemAsync(Guid fileID)
        {
            var mediaLibraryItem = await _mediaLibraryRepository.GetMediaItemAsync(fileID);
            if (mediaLibraryItem == null)
            {
                return null;
            }
            return new MediaItemDTO(mediaLibraryItem);
        }

        public async Task<IPagedEnumerable<MediaItemDTO>> GetMediaItemsAsync(MediaItemListingRequest request)
        {
            var mediaItems = await _mediaLibraryRepository.GetMediaItemsAsync(request.Page, request.PageSize,
                x => x.CategoryID == request.CategoryID &&
                 (string.IsNullOrWhiteSpace(request.Search) || x.FileID.Equals(request.Search) || x.FileName.Contains(request.Search)
                 && string.IsNullOrWhiteSpace(request.Extensions) || x.Extension != null && x.Extension.Equals(request.Extensions))
             );

            return mediaItems.Select(x => new MediaItemDTO(x));
        }

        public async Task<IEnumerable<MediaItemDTO>> GetMediaItemsAsync(Guid[] fileIds)
        {
            if (!fileIds.Any())
                return Enumerable.Empty<MediaItemDTO>();
            var mediaItems = await _mediaLibraryRepository.GetMediaItemsAsync(x => fileIds.Contains(x.FileID));

            return mediaItems.Select(x => new MediaItemDTO(x));
        }

        public async Task<IEnumerable<MediaItemDTO>> InsertMediaItemAsync(MediaItemInsertRequest request)
        {
            var category = await _mediaLibraryCategoryRepository.GetMediaLibraryCategoryAsync(request.CategoryID);
            if (category == null)
                return Enumerable.Empty<MediaItemDTO>();

            var mediaFiles = new List<MediaLibrary>();
            string categoryPathFolder = MediaLibraryHelper.GetDirectPath(category.FolderName);
            try
            {
                if (!Directory.Exists(categoryPathFolder))
                {
                    Directory.CreateDirectory(categoryPathFolder);
                }

                foreach (var file in request.Files)
                {
                    var fileGUID = Guid.NewGuid();
                    string fileName = $"{fileGUID}{Path.GetExtension(file.FileName)}";
                    mediaFiles.Add(new MediaLibrary
                    {
                        CategoryID = category.CategoryID,
                        Extension = Path.GetExtension(file.FileName),
                        FileName = file.FileName,
                        Title = file.Name,
                        Size = file.Length,
                        FileID = fileGUID
                    });

                    using (var stream = new FileStream(Path.Combine(categoryPathFolder, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                var mediaItemInserts = await _mediaLibraryRepository.InsertMediaItems(mediaFiles);
                return mediaItemInserts.Select(x => new MediaItemDTO(x));
            }
            catch
            {
                foreach (var mediaFile in mediaFiles)
                {
                    File.Delete(Path.Combine(categoryPathFolder, $"{mediaFile.FileID}{mediaFile.Extension}"));
                }
                throw;
            }

        }

        public async Task<int> RenameMediaFolderAsync(int categoryId, MediaFolderRequest mediaFolderRequest)
        {
            return await _mediaLibraryCategoryRepository.UpdateCategory(categoryId, mediaFolderRequest.FolderName);
        }

        public async Task<MediaItemDTO?> UpdateMediaItemAsync(MediaItemUpdateRequest request)
        {
            var mediaItem = await _mediaLibraryRepository.GetMediaItemAsync(request.FileID);
            if (mediaItem == null || request.File.Length <= 0)
                return null;

            var oldPath = MediaLibraryHelper.GetDirectMediaFilePath(mediaItem);
            mediaItem.FileName = request.File.FileName;
            mediaItem.Size = request.File.Length;
            mediaItem.Title = request.File.Name;
            mediaItem.Extension = Path.GetExtension(request.File.FileName);

            try
            {
                var mediaUpdated = await _mediaLibraryRepository.UpdateMediaItem(mediaItem);
                string categoryPathFolder = MediaLibraryHelper.GetDirectPath(mediaUpdated.MediaLibraryCategory.FolderName);
                File.Move(oldPath, oldPath + ".temp");
                if (!Directory.Exists(categoryPathFolder))
                {
                    Directory.CreateDirectory(categoryPathFolder);
                }

                using (var stream = new FileStream(MediaLibraryHelper.GetDirectMediaFilePath(mediaUpdated), FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                File.Delete(oldPath + ".temp");
                return new MediaItemDTO(mediaUpdated);
            }
            catch
            {
                if (Directory.Exists(oldPath))
                {
                    File.Delete(oldPath);
                }

                File.Move(oldPath + ".temp", oldPath);
                throw;
            }
        }
    }
}
