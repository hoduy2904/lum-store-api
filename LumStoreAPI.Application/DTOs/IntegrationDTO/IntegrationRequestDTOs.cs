using LumStoreAPI.Application.DTOs.ShiprelayDTO;

namespace LumStoreAPI.Application.DTOs.IntegrationDTO;

public class CreateShipmentRequest
{
    public string OrderRef { get; set; } = default!;
    public decimal ShipmentTotalCost { get; set; }
    public int PackageRef { get; set; } = 1;
    public string? Type { get; set; }                       // b2c | b2b | transfer
    public string RecipientName { get; set; } = default!;
    public string? Company { get; set; }
    public string Address1 { get; set; } = default!;
    public string? Address2 { get; set; }
    public string City { get; set; } = default!;
    public string? State { get; set; }
    public string Zip { get; set; } = default!;
    public string Country { get; set; } = "US";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public string? ShippingSelectedRef { get; set; }
    public List<ShiprelayItemDTO> Items { get; set; } = [];
}

public class GetRatesRequest
{
    public string RecipientName { get; set; } = default!;
    public string Address1 { get; set; } = default!;
    public string? Address2 { get; set; }
    public string City { get; set; } = default!;
    public string Region { get; set; } = default!;          // state/province
    public string? Country { get; set; }
    public string Zip { get; set; } = default!;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? SessionId { get; set; }
    public List<ShiprelayItemDTO> Items { get; set; } = [];
}
