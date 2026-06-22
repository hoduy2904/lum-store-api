namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class ProductByColorItemDTO
{
    public int NodeID { get; set; }
    public string NodeAlias { get; set; } = default!;
    public string RelativeUrl { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal PriceDiscount { get; set; }
    public string? Image { get; set; }
    public MatchedVariantDTO MatchedVariant { get; set; } = default!;
}

public class MatchedVariantDTO
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = default!;
    public string? Color { get; set; }
    public string? ColorImage { get; set; }
    public string SKU { get; set; } = default!;
    public int Stock { get; set; }
}
