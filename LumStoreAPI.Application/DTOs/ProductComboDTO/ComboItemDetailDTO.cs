namespace LumStoreAPI.Application.DTOs.ProductComboDTO;

public class ComboItemDetailDTO
{
    public int VariantId { get; set; }
    public string VariantName { get; set; } = default!;
    public string ProductName { get; set; } = default!;
    public decimal UnitPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
}

public class ComboPriceResult
{
    public decimal TotalPrice { get; set; }
    public int ComboStock { get; set; }
    public IEnumerable<ComboItemDetailDTO> Items { get; set; } = [];
}
