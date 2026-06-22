using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Application.DTOs.ProductVariantDTO;

public record class ProductVariantClientGetDTO : ProductVariantGetDTO
{
    public new string[] Images { get; set; } = [];
    public string? ColorImage { get; set; }

    public ProductVariantClientGetDTO(ProductVariant productVariant, MediaItemDTO[] mediaItemDTOs, string? colorImageUrl = null) : base(productVariant)
    {
        this.VariantId = productVariant.ItemID;
        this.SKU = productVariant.SKU;
        this.ProductID = productVariant.ProductID;
        this.UPC = productVariant.UPC;
        this.Stock = productVariant.Stock;
        if (mediaItemDTOs != null && mediaItemDTOs.Any())
        {
            this.Images = mediaItemDTOs.Select(x => x.FileURL).ToArray();
        }
        this.Color = productVariant.Color?.ColorValue;
        this.ColorImageId = productVariant.Color?.ColorImageId;
        this.ColorImage = colorImageUrl;
        this.VariantName = productVariant.VariantName;
        this.ColorId = productVariant.ColorId;
        this.ParentId = productVariant.ParentId;
    }
}
