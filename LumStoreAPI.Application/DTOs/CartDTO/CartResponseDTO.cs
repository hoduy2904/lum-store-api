using LumStoreAPI.Application.DTOs.DocumentPageDTO;

namespace LumStoreAPI.Application.DTOs.CartDTO;

public class CartResponseDTO
{
    public IEnumerable<CartItemDTO> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public int ItemCount { get; set; }
}

public class CartItemDTO
{
    public int CartItemId { get; set; }
    public int NodeID { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
    public DocumentClientGetDTO? Product { get; set; }
}

public class CartAddResultDTO
{
    public int CartItemId { get; set; }
    public int NodeID { get; set; }
    public int Quantity { get; set; }
    public int? VariantId { get; set; }
}
