using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.CartDTO;

public class CartAddRequest
{
    [Required]
    public int NodeID { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public int? VariantId { get; set; }
}

public class CartUpdateRequest
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}

public class CartSyncRequest
{
    public CartSyncItem[] Items { get; set; } = [];
}

public class CartSyncItem
{
    public int NodeID { get; set; }
    public int Quantity { get; set; } = 1;
    public int? VariantId { get; set; }
}
