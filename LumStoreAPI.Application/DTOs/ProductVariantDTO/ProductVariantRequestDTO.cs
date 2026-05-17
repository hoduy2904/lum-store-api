using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Application.DTOs.ProductVariantDTO;

public record class ProductVariantRequestDTO : ProductVariantUpdateDTO
{
    public int ProductId { get; set; }

    public ProductVariant GetEntity()
    {
        return new ProductVariant
        {
            ColorId = ColorId,
            ProductID = ProductId,
            SKU = SKU,
            Stock = 0,
            UPC = UPC,
            VariantName = VariantName,
            Images = Images,
            ParentId = ParentId
        };
    }
}
