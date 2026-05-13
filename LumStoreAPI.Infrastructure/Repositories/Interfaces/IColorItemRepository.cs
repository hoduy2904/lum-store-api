using System;
using System.Linq.Expressions;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using Microsoft.EntityFrameworkCore.Query;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface IColorItemRepository
{
    IQueryable<ColorItem> GetColors();
    Task<IPagedEnumerable<ColorItem>> GetColorsAsync(int page, int pageSize, Expression<Func<ColorItem, bool>>? where = null);
    Task<ColorItem?> GetColorAsync(int colorId);
    Task<ColorItem> InsertColorAsync(ColorItem color);
    Task<bool> UpdateColorAsync(int colorId, Action<UpdateSettersBuilder<ColorItem>> action);
    Task<bool> DeleteColorAsync(int colorId);
    Task<bool> MoveToPositionAsync(int colorId, int newPosition);
    Task<int> DeleteColorsAsync(params int[] colorIds);
}
