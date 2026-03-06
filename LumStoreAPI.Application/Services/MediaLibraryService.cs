using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.Services
{
    internal class MediaLibraryService : IMediaLibraryService
    {
        private readonly IMediaLibraryRepository _mediaLibraryRepository;
        public MediaLibraryService(IMediaLibraryRepository mediaLibraryRepository)
        {
            _mediaLibraryRepository = mediaLibraryRepository;
        }
        public async Task<MediaItem?> GetMediaItemAsync(Guid fileID)
        {
            var mediaLibraryItem = await _mediaLibraryRepository.GetMediaItemAsync(fileID);
            if (mediaLibraryItem == null)
            {
                return null;
            }
            return new MediaItem(mediaLibraryItem);
        }

        public async Task<IEnumerable<MediaItem>> GetMediaItemsAsync(Guid[] fileIDs)
        {
            var mediaLibraryItems = await _mediaLibraryRepository.GetMediaItemsAsync(x => fileIDs.Contains(x.FileID));
            return mediaLibraryItems.Select(x => new MediaItem(x));
        }

        public async Task<IPagedEnumerable<MediaItem>> GetMediaItemsAsync(int categoryId, int page, int pageSize, string? q = "")
        {
            var mediaItems = await _mediaLibraryRepository.GetMediaItemsAsync(page, pageSize,
                x => x.CategoryID == categoryId &&
                (string.IsNullOrEmpty(q) || x.FileName.Contains(q) || (x.Extension != null && x.Extension.Equals(q))));

            return mediaItems.Select(x => new MediaItem(x));
        }

        public async Task<MediaItem> InsertMediaItemAsync(MediaItem mediaItem)
        {
            var mediaLibrary = await _mediaLibraryRepository.InsertMediaItem(new MediaLibrary
            {
                CategoryID = mediaItem.CategoryID,
                FileName = mediaItem.FileName,
                Extension = mediaItem.Extension,
                Height = mediaItem.Height,
                Width = mediaItem.Width,
                Size = mediaItem.Size,
                Title = mediaItem.Title
            });

            return new MediaItem(mediaLibrary);

        }

        public async Task<IEnumerable<MediaItem>> InsertMediaItemsAsync(IEnumerable<MediaItem> mediaItems)
        {
            var mediaItemInserts = await _mediaLibraryRepository.InsertMediaItems(mediaItems.Select(x => new MediaLibrary
            {
                CategoryID = x.CategoryID,
                FileName = x.FileName,
                Extension = x.Extension,
                Height = x.Height,
                Width = x.Width,
                Size = x.Size,
                Title = x.Title
            }).ToArray());

            return mediaItemInserts.Select(x => new MediaItem(x));
        }
    }
}
