using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.ProductComboDTO;

public class ComboItemDetailDTO
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public int ProductID { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public int Stock { get; set; }
    [JsonIgnore]
    public int ShiprelayId { get; set; }
    public string[] Images { get; set; } = [];
    public string? SKU { get; set; }
    public string? UPC { get; set; }
    public string? Color { get; set; }
    public Guid? ColorImageId { get; set; }
    public string? ColorImage { get; set; }
    public int? ColorId { get; set; }
    public int? ParentId { get; set; }
}

public class ComboPriceResult
{
    public decimal SubTotal { get; set; }
    public decimal TotalPrice { get; set; }
    public int ComboStock { get; set; }
    public IEnumerable<ComboItemDetailDTO> Items { get; set; } = [];
}
