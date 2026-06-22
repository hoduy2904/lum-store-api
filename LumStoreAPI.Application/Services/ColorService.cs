using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services;

internal class ColorService(
    IColorCategoryRepository colorCategoryRepository,
    IMediaService mediaService
) : IColorService
{
    private readonly IColorCategoryRepository _colorCategoryRepository = colorCategoryRepository;
    private readonly IMediaService _mediaService = mediaService;

    public async Task<IEnumerable<RelatedContentKeyValue>> GetGroupColorsAsync()
    {
        var categories = await _colorCategoryRepository.GetColorCategories()
            .Include(x => x.Colors)
            .AsNoTrackingWithIdentityResolution()
            .ToArrayAsync();

        var allImageIds = categories
            .SelectMany(c => c.Colors)
            .Where(c => c.ColorImageId.HasValue)
            .Select(c => c.ColorImageId!.Value)
            .Distinct()
            .ToArray();

        var mediaMap = allImageIds.Length > 0
            ? (await _mediaService.GetMediaItemsAsync(allImageIds))
                .ToDictionary(m => m.FileID, m => m.FileURL)
            : new Dictionary<Guid, string>();

        return categories.Select(x => new RelatedContentKeyValue
        {
            Key = x.CategoryName,
            Value = x.ItemID,
            RelatedData = x.Colors.Select(c => new ContentKeyValue
            {
                Key = c.ColorImageId.HasValue && mediaMap.TryGetValue(c.ColorImageId.Value, out var url)
                      ? url
                      : (c.ColorValue ?? ""),
                Value = c.ItemID
            })
        });
    }
}
