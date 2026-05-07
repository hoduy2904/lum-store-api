using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Application.DTOs.AddressDTO;

public class AddressDTO
{
    public int Id { get; set; }
    public string Phone { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string ZipCode { get; set; } = default!;
    public string Details { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public AddressDTO(CustomerAddress a)
    {
        Id = a.ItemID;
        Phone = a.Phone;
        Address = a.Address ?? string.Empty;
        City = a.City;
        State = a.State;
        ZipCode = a.ZipCode ?? string.Empty;
        Details = a.Details ?? string.Empty;
        IsDefault = a.IsDefault;
    }
}
