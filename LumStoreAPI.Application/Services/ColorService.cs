using System;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

internal class ColorService(
    IColorCategoryRepository colorCategoryRepository
) : IColorService
{
    private readonly IColorCategoryRepository _colorCategoryRepository = colorCategoryRepository;
    public async Task<IEnumerable<RelatedContentKeyValue>> GetGroupColorsAsync()
    {
        return await _colorCategoryRepository.GetColorCategories()
         .Include(x => x.Colors)
         .AsNoTrackingWithIdentityResolution()
         .Where(x => x.Colors.Any(c => c.ProductVariants.Any()))
         .Select(x => new RelatedContentKeyValue
         {
             Key = x.CategoryName,
             Value = x.ItemID,
             RelatedData = x.Colors.Select(c => new ContentKeyValue
             {
                 Key = c.ColorValue,
                 Value = c.ItemID
             })
         }).ToArrayAsync();

    }
}
