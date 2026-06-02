using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class ProductClientDTO
{
    public string ProductName { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public bool IsBestSeller { get; set; }
    public bool IsCombo { get; set; }
    public decimal? Price { get; set; }
    public decimal? PriceDiscount { get; set; }
    public string[] Images { get; set; } = [];
    public double Length { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public bool IsFoldable { get; set; }
    public bool IsAlcoholic { get; set; }
    public bool IsHazmat { get; set; }
    public bool IsNeedBox { get; set; }
    public bool IsFragile { get; set; }
    public IEnumerable<ProductVariantClientGetDTO> ProductVariants { get; set; } = [];
    public IEnumerable<ProductDiscountTierDTO> DiscountRules { get; set; } = [];
    public IEnumerable<ContentKeyValue> Accordions { get; set; } = [];

    public ProductClientDTO(Product product)
    {
        this.ProductName = product.ProductName;
        this.ShortDescription = product.ShortDescription;
        this.Description = product.Description;
        this.IsBestSeller = product.IsBestSeller;
        this.Price = product.Price;
        this.PriceDiscount = product.PriceDiscount;
        this.Length = product.Length;
        this.Width = product.Width;
        this.Height = product.Height;
        this.Weight = product.Weight;
        this.IsFoldable = product.IsFoldable;
        this.IsAlcoholic = product.IsAlcoholic;
        this.IsHazmat = product.IsHazmat;
        this.IsNeedBox = product.IsNeedBox;
        this.IsFragile = product.IsFragile;
        this.IsCombo = product.IsCombo;
    }
}
