using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Application.DTOs.ProductVariantDTO;

public record class ProductVariantGetDTO
{
    public int VariantId { get; set; }
    public string SKU { get; set; } = default!;
    public int ProductID { get; set; }
    public string UPC { get; set; } = default!;
    public int Stock { get; set; }
    public string[] Images { get; set; } = [];
    public string? Color { get; set; }
    public int? ColorId { get; set; }
    public string VariantName { get; set; } = default!;
    public int? ParentId { get; set; }

    public ProductVariantGetDTO(ProductVariant productVariant, MediaItemDTO[]? mediaItemDTO = null)
    {
        this.VariantId = productVariant.ItemID;
        this.SKU = productVariant.SKU;
        this.ProductID = productVariant.ProductID;
        this.UPC = productVariant.UPC;
        this.Stock = productVariant.Stock;
        if (mediaItemDTO != null && mediaItemDTO.Any())
        {
            this.Images = mediaItemDTO.Select(x => x.FileURL).ToArray();
        }
        this.Color = productVariant.Color?.ColorValue;
        this.VariantName = productVariant.VariantName;
        this.ColorId = productVariant.ColorId;
        this.ParentId = productVariant.ParentId;
    }
}
