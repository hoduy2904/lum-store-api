using System;
using System.Linq.Expressions;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations;

public class ColorItemRepository(
    LumStoreContext lumStoreContext
) : IColorItemRepository
{
    private readonly LumStoreContext _lumStoreContext = lumStoreContext;

    public async Task<bool> DeleteColorAsync(int colorId)
    {
        return (await DeleteColorsAsync([colorId])) > 0;
    }

    public Task<int> DeleteColorsAsync(params int[] colorIds)
    {
        return _lumStoreContext.ColorItems
         .Where(x => colorIds.Contains(x.ItemID)).ExecuteDeleteAsync();
    }

    public IQueryable<ColorItem> GetColors()
    {
        return _lumStoreContext.ColorItems;
    }

    public Task<ColorItem?> GetColorAsync(int colorId)
    {
        return _lumStoreContext.ColorItems.AsNoTracking().FirstOrDefaultAsync(x => x.ItemID == colorId);
    }

    public async Task<IPagedEnumerable<ColorItem>> GetColorsAsync(int page, int pageSize, Expression<Func<ColorItem, bool>>? where = null)
    {
        where ??= x => true;
        var colorsDb = GetColors().AsNoTracking()
         .Where(where);

        int totalRecords = await colorsDb.CountAsync();

        var categories = await colorsDb
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedEnumerable<ColorItem>(categories, totalRecords);
    }

    public async Task<ColorItem> InsertColorAsync(ColorItem color)
    {
        var maxItemOrder = (await GetColors().MaxAsync(x => (int?)x.ItemOrder)) ?? 0;
        color.ItemOrder = maxItemOrder;
        _lumStoreContext.ColorItems.Add(color);
        await _lumStoreContext.SaveChangesAsync();
        return color;
    }

    public async Task<bool> MoveToPositionAsync(int colorId, int newPosition)
    {
        var count = await _lumStoreContext.ColorItems.Where(x => x.ItemOrder >= newPosition)
             .ExecuteUpdateAsync(x => x.SetProperty(p => p.ItemOrder, p => p.ItemID == colorId ? newPosition : p.ItemOrder + 1));

        return count > 0;
    }

    public async Task<bool> UpdateColorAsync(int colorId, Action<UpdateSettersBuilder<ColorItem>> action)
    {
        var count = await _lumStoreContext.ColorItems
         .Where(x => x.ItemID == colorId).ExecuteUpdateAsync(action);
        return count > 0;
    }
}
