using LumStoreAPI.Core.Entities.Customers;

namespace LumStoreAPI.Application.DTOs.AddressDTO;

public class AddressDTO
{
    public int Id { get; set; }
    public string Phone { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string State { get; set; } = default!;
    public string Details { get; set; } = string.Empty;
    public bool IsDefault { get; set; }

    public AddressDTO(CustomerAddress a)
    {
        Id = a.ItemID;
        Phone = a.Phone;
        Street = a.Street;
        City = a.City;
        State = a.State;
        Details = a.Details ?? string.Empty;
        IsDefault = a.IsDefault;
    }
}
