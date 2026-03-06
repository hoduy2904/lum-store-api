using LumStoreAPI.Core.Entities.Systems;
using System.Linq.Expressions;

namespace LumStoreAPI.Core.Interfaces.Repositories
{
    public interface IMediaLibraryCategoryRepository
    {
        Task<MediaLibraryCategory?> GetMediaLibraryCategoryAsync(int categoryID);
        Task<IEnumerable<MediaLibraryCategory>> GetMediaLibraryCategories(Expression<Func<MediaLibraryCategory, bool>>? where = null);
        Task<MediaLibraryCategory> InsertCategory(MediaLibraryCategory category);
        Task<int> UpdateCategory(int categoryID, string categoryName);
        Task<int> DeleteCategories(Expression<Func<MediaLibraryCategory, bool>> where);
        Task<int> DeleteCategory(int categoryID);
    }
}
