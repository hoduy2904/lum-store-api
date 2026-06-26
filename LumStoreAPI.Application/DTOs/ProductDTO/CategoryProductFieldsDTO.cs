namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class CategoryProductFieldsDTO
{
    public string ProductName { get; set; } = default!;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsCombo { get; set; }
    public bool IsExpand { get; set; }
    public decimal Price { get; set; }
    public decimal PriceDiscount { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string[] Images { get; set; } = [];
    /// <summary>Sum of all variant stocks (0 if no variants).</summary>
    public int Stock { get; set; }
    /// <summary>Min stock across all combo variant items. Only meaningful when IsCombo = true.</summary>
    public int ComboStock { get; set; }
    public List<CategoryProductVariantDTO> ProductVariants { get; set; } = [];
    public IEnumerable<ProductDiscountTierDTO> DiscountRules { get; set; } = [];
}

public class CategoryProductVariantDTO
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = default!;
    public string? Color { get; set; }
    public string? ColorImage { get; set; }
    public Guid? ColorImageId { get; set; }
    public int Stock { get; set; }
    public string SKU { get; set; } = default!;
    public string[] Images { get; set; } = [];
}
