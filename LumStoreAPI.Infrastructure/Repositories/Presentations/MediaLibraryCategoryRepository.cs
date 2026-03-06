using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
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

        public async Task<IEnumerable<MediaLibraryCategory>> GetMediaLibraryCategories(Expression<Func<MediaLibraryCategory, bool>>? where = null)
        {
            return await _lumStoreContext.MediaLibraryCategories
                .AsNoTracking()
                .Where(where ?? (x => true))
                .ToArrayAsync();
        }

        public async Task<MediaLibraryCategory?> GetMediaLibraryCategoryAsync(int categoryID)
        {
            return await _lumStoreContext.MediaLibraryCategories.FindAsync(categoryID);
        }

        public async Task<MediaLibraryCategory> InsertCategory(MediaLibraryCategory category)
        {
            _lumStoreContext.MediaLibraryCategories.Add(category);
            await _lumStoreContext.SaveChangesAsync();
            return category;
        }

        public Task<int> UpdateCategory(int categoryID, string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new InvalidDataException("Category name cannot null or empty");
            }

            return _lumStoreContext.MediaLibraryCategories
                  .Where(x => x.CategoryID == categoryID)
                  .ExecuteUpdateAsync(x => x.SetProperty(p => p.CategoryName, categoryName));
        }
    }
}
