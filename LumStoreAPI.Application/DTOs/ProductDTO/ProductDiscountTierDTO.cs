namespace LumStoreAPI.Application.DTOs.ProductDTO;

public class ProductDiscountTierDTO
{
    public string RuleName { get; set; } = default!;
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal DiscountAmount { get; set; }
}
