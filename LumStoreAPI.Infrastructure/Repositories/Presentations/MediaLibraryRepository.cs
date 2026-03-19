using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class MediaLibraryRepository : IMediaLibraryRepository
    {
        private readonly LumStoreContext _lumStoreContext;
        public MediaLibraryRepository(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public Task<int> DeleteMediaItem(Guid fileID)
        {
            return this.DeleteMediaItems(x => x.FileID == fileID);
        }

        public async Task<int> DeleteMediaItems(Expression<Func<MediaLibrary, bool>> where)
        {
            var directPaths = await this.GetMediaDirectFilePaths(where);
            var result = await _lumStoreContext.MediaLibraries.Where(where).ExecuteDeleteAsync();
            if (result > 0)
            {
                Parallel.ForEach(directPaths, async path =>
                {
                    File.Delete(path);
                });
            }

            return result;
        }

        public async Task<IEnumerable<string>> GetMediaDirectFilePaths(Guid[] fileIds)
        {
            var paths = await _lumStoreContext
                  .MediaLibraries
                  .Where(x => fileIds.Contains(x.FileID))
                  .Join(_lumStoreContext.MediaLibraryCategories,
                  md => md.CategoryID,
                  fd => fd.CategoryID,
                  (md, fd) => new { md.Extension, md.FileID, fd.FolderName })
                  .ToListAsync();
            return paths.Select(x => MediaLibraryHelper.GetDirectPath(Path.Combine(x.FolderName, $"{x.FileID}{x.Extension}")));
        }

        public async Task<IEnumerable<string>> GetMediaDirectFilePaths(Expression<Func<MediaLibrary, bool>>? where = null)
        {
            where ??= x => true;
            var paths = await _lumStoreContext
                  .MediaLibraries
                  .Where(where)
                  .Join(_lumStoreContext.MediaLibraryCategories,
                  md => md.CategoryID,
                  fd => fd.CategoryID,
                  (md, fd) => new { md.Extension, md.FileID, fd.FolderName })
                  .ToListAsync();
            return paths.Select(x => MediaLibraryHelper.GetDirectPath(Path.Combine(x.FolderName, $"{x.FileID}{x.Extension}")));
        }

        public async Task<MediaLibrary?> GetMediaItemAsync(Guid fileID)
        {
            return await _lumStoreContext.MediaLibraries.Include(x => x.MediaLibraryCategory).FirstOrDefaultAsync(x => x.FileID == fileID);
        }

        public async Task<IEnumerable<MediaLibrary>> GetMediaItemsAsync(Expression<Func<MediaLibrary, bool>>? where = null)
        {
            return await _lumStoreContext.MediaLibraries
                .AsNoTracking()
                .Include(x => x.MediaLibraryCategory)
                .Where(where ?? (x => true))
                .ToListAsync();
        }

        public Task<IPagedEnumerable<MediaLibrary>> GetMediaItemsAsync(int page, int pageSize, Expression<Func<MediaLibrary, bool>>? where = null)
        {
            return _lumStoreContext.MediaLibraries
                .AsNoTracking()
                .Where(where ?? (x => true))
                .Include(x => x.MediaLibraryCategory)
                .OrderByDescending(x => x.UpdatedAt)
                .AsQueryable()
                .GetPagedAsync(page, pageSize);
        }

        public async Task<MediaLibrary> InsertMediaItem(MediaLibrary mediaLibrary)
        {
            _lumStoreContext.MediaLibraries.Add(mediaLibrary);
            await _lumStoreContext.SaveChangesAsync();
            return mediaLibrary;
        }

        public async Task<IEnumerable<MediaLibrary>> InsertMediaItems(IEnumerable<MediaLibrary> mediaLibraries)
        {
            _lumStoreContext.AddRange(mediaLibraries);
            await _lumStoreContext.SaveChangesAsync();
            return mediaLibraries;
        }

        public async Task<MediaLibrary> UpdateMediaItem(MediaLibrary mediaLibrary)
        {
            _lumStoreContext.MediaLibraries.Update(mediaLibrary);
            await _lumStoreContext.SaveChangesAsync();
            return mediaLibrary;
        }
    }
}
