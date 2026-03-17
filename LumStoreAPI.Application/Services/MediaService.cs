using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.Services
{
    internal class MediaService : IMediaService
    {
        private readonly IMediaLibraryService _mediaLibraryService;
        private readonly IMediaLibraryCategoryRepository _mediaLibraryCategoryRepository;
        public MediaService(IMediaLibraryService mediaLibraryService, IMediaLibraryCategoryRepository mediaLibraryCategoryRepository)
        {
            _mediaLibraryService = mediaLibraryService;
            _mediaLibraryCategoryRepository = mediaLibraryCategoryRepository;
        }
        public Task<MediaItem?> GetMediaItemAsync(Guid fileID)
        {
            return _mediaLibraryService.GetMediaItemAsync(fileID);
        }

        public async Task<IEnumerable<MediaItem>> InsertMediaItemAsync(MediaItemInsertRequest request)
        {
            var category = await _mediaLibraryCategoryRepository.GetMediaLibraryCategoryAsync(request.CategoryID);
            if (category == null)
                return Enumerable.Empty<MediaItem>();

            var mediaFiles = new List<MediaItem>();
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
                    mediaFiles.Add(new MediaItem
                    {
                        CategoryID = category.CategoryID,
                        Extension = Path.GetExtension(file.FileName),
                        FileName = file.FileName,
                        Title = file.Name,
                        Size = file.Length,
                        FileID = fileGUID,
                    });

                    using (var stream = new FileStream(Path.Combine(categoryPathFolder, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                return await _mediaLibraryService.InsertMediaItemsAsync(mediaFiles);
            }
            catch (Exception ex)
            {
                foreach (var mediaFile in mediaFiles)
                {
                    File.Delete(Path.Combine(categoryPathFolder, $"{mediaFile.FileID}{mediaFile.Extension}"));
                }
                throw;
            }

        }

        public Task<MediaItem> UpdateMediaItemAsync(MediaItemUpdateRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
