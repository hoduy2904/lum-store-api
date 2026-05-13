using System;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

public class ColorCategoryRepository(
    LumStoreContext lumStoreContext
) : IColorCategoryRepository
{
    private readonly LumStoreContext _lumStoreContext = lumStoreContext;
    public Task<int> DeleteCategoriesAsync(params int[] categoryIds)
    {
        return _lumStoreContext.ColorCategories
         .Where(x => categoryIds.Contains(x.ItemID)).ExecuteDeleteAsync();
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        return (await DeleteCategoriesAsync([categoryId])) > 0;
    }

    public Task<ColorCategory?> GetCategoryAsync(int categoryId)
    {
        return _lumStoreContext.ColorCategories.AsNoTracking().FirstOrDefaultAsync(x => x.ItemID == categoryId);
    }

    public IQueryable<ColorCategory> GetColorCategories()
    {
        return _lumStoreContext.ColorCategories;
    }

    public async Task<IPagedEnumerable<ColorCategory>> GetColorCategoriesAsync(int page, int pageSize, string query = "")
    {
        var categoriesDb = GetColorCategories().AsNoTracking()
         .Where(x => string.IsNullOrWhiteSpace(query) || x.CategoryName.Contains(query));

        int totalRecords = await categoriesDb.CountAsync();

        var categories = await categoriesDb
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedEnumerable<ColorCategory>(categories, totalRecords);
    }

    public async Task<ColorCategory> InsertCategoryAsync(string categoryName)
    {
        var maxItemOrder = (await GetColorCategories().MaxAsync(x => (int?)x.ItemOrder)) ?? 0;
        var category = new ColorCategory
        {
            CategoryName = categoryName,
            ItemOrder = maxItemOrder
        };

        _lumStoreContext.ColorCategories.Add(category);
        await _lumStoreContext.SaveChangesAsync();
        return category;
    }

    public async Task<bool> MoveToPositionAsync(int categoryid, int newPosition)
    {
        var count = await _lumStoreContext.ColorCategories.Where(x => x.ItemOrder >= newPosition)
             .ExecuteUpdateAsync(x => x.SetProperty(p => p.ItemOrder, p => p.ItemID == categoryid ? newPosition : p.ItemOrder + 1));

        return count > 0;
    }

    public async Task<bool> UpdateCategoryAsync(int categoryId, Action<UpdateSettersBuilder<ColorCategory>> action)
    {
        var count = await _lumStoreContext
        .ColorCategories
        .Where(x => x.ItemID == categoryId).ExecuteUpdateAsync(action);
        return count > 0;
    }
}
