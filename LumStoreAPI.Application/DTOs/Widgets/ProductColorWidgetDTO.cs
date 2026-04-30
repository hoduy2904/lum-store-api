using System;
using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;

namespace LumStoreAPI.Application.DTOs.Widgets;

public record class ProductColorWidgetDTO(IEnumerable<ContentKeyValue> colors);
