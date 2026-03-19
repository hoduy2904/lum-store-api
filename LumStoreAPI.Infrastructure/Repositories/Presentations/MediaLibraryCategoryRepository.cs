using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure.Extensions;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class MediaLibraryCategoryRepository : IMediaLibraryCategoryRepository
    {
        private readonly LumStoreContext _lumStoreContext;
        public MediaLibraryCategoryRepository(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public Task<int> DeleteCategories(Expression<Func<MediaLibraryCategory, bool>> where)
        {
            return _lumStoreContext.MediaLibraryCategories.Where(where).ExecuteDeleteAsync();
        }

        public Task<int> DeleteCategory(int categoryID)
        {
            return this.DeleteCategories(x => x.CategoryID == categoryID);
        }

        public async Task<IEnumerable<MediaLibraryCategory>> GetMediaLibraryCategoriesAsync(Expression<Func<MediaLibraryCategory, bool>>? where = null)
        {
            return await _lumStoreContext.MediaLibraryCategories
                .AsNoTracking()
                .Where(where ?? (x => true))
                .ToArrayAsync();
        }

        public async Task<IPagedEnumerable<MediaLibraryCategory>> GetMediaLibraryCategoriesAsync(int page, int pageSize, Expression<Func<MediaLibraryCategory, bool>>? where = null)
        {
            var categories = _lumStoreContext.MediaLibraryCategories.AsQueryable();
            if (where != null)
            {
                categories = categories.Where(where);
            }
            int count = await categories.CountAsync();
            var data = await categories.OrderByDescending(x => x.CategoryID)
                        .Take(pageSize)
                        .Skip((page - 1) * pageSize)
                        .ToArrayAsync();

            return data.AsPagedEnumerable(count);
        }

        public async Task<MediaLibraryCategory?> GetMediaLibraryCategoryAsync(int categoryID)
        {
            return await _lumStoreContext.MediaLibraryCategories.FindAsync(categoryID);
        }

        public async Task<IEnumerable<string>> GetMediaLibraryFolderPaths(int[] categoryIds)
        {
            var data = await _lumStoreContext.MediaLibraryCategories.Where(x => categoryIds.Contains(x.CategoryID)).Select(x => x.FolderName).ToArrayAsync();
            return data.Select(x => MediaLibraryHelper.GetDirectPath(x));
        }

        public async Task<MediaLibraryCategory> InsertCategory(MediaLibraryCategory category)
        {
            _lumStoreContext.MediaLibraryCategories.Add(category);
            await _lumStoreContext.SaveChangesAsync();
            Directory.CreateDirectory(MediaLibraryHelper.GetDirectPath(category.FolderName));
            return category;
        }

        public async Task<int> UpdateCategory(int categoryID, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new InvalidDataException("Category name cannot null or empty");
            }
            var oldCategory = await GetMediaLibraryCategoryAsync(categoryID);
            if (oldCategory == null)
                return 0;

            var result = await _lumStoreContext.MediaLibraryCategories
                  .Where(x => x.CategoryID == categoryID)
                  .ExecuteUpdateAsync(x =>
                  x.SetProperty(p => p.CategoryName, categoryName)
                  .SetProperty(p => p.FolderName, categoryName.Slug));

            if (result > 0)
            {
                File.Move(MediaLibraryHelper.GetDirectPath(oldCategory.FolderName), MediaLibraryHelper.GetDirectPath(categoryName.Slug));
            }
            return result;
        }
    }
}
