using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.WishlistDTO;

public class WishlistAddRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int NodeID { get; set; }
}

public class WishlistSyncRequest
{
    public int[] NodeIDs { get; set; } = [];
}
