using System;
using LumStoreAPI.Application.DTOs.StoreDTO;

namespace LumStoreAPI.Application.DTOs.QueryDTOs;

public class ProductListingFeatureDTO
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public IEnumerable<StoreCategoryDTO> Categories { get; set; } = [];
    public Dictionary<string, ContentKeyValue> Filters { get; set; } = [];
}
