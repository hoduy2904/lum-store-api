using System;

namespace LumStoreAPI.Application.DTOs.ProductVariantDTO;

public record class ProductVariantUpdateDTO
{
    public string SKU { get; set; } = default!;
    public string UPC { get; set; } = default!;
    public Guid[] Images { get; set; } = [];
    public int ColorId { get; set; }
    public string VariantName { get; set; } = default!;
}
