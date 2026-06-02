using System;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.DTOs.QueryDTOs;

public class ProductListingFeatureDTO
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public IEnumerable<StoreCategoryDTO> Categories { get; set; } = [];
    public Dictionary<string, ContentKeyValue> Filters { get; set; } = [];
}
