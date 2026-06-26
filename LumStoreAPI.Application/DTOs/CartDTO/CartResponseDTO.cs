using LumStoreAPI.Application.DTOs.DocumentPageDTO;

namespace LumStoreAPI.Application.DTOs.CartDTO;

public class CartResponseDTO
{
    public IEnumerable<CartItemDTO> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public int ItemCount => Items.Sum(x => x.Quantity);
}

public class CartItemDTO
{
    public int CartItemId { get; set; }
    public int NodeID { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
    public string? VariantName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal BasePrice { get; set; }
    public DocumentClientGetDTO? Product { get; set; }
}

public class CartAddResultDTO
{
    public int CartItemId { get; set; }
    public int NodeID { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
}
