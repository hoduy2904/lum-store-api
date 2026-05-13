using System;
using System.Linq.Expressions;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using Microsoft.EntityFrameworkCore.Query;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces;

public interface IColorCategoryRepository
{
    IQueryable<ColorCategory> GetColorCategories();
    Task<IPagedEnumerable<ColorCategory>> GetColorCategoriesAsync(int page, int pageSize, string query = "");
    Task<ColorCategory?> GetCategoryAsync(int categoryId);
    Task<ColorCategory> InsertCategoryAsync(string categoryName);
    Task<bool> UpdateCategoryAsync(int categoryId, Action<UpdateSettersBuilder<ColorCategory>> action);
    Task<bool> DeleteCategoryAsync(int categoryId);
    Task<bool> MoveToPositionAsync(int categoryid, int newPosition);
    Task<int> DeleteCategoriesAsync(params int[] categoryIds);
}
