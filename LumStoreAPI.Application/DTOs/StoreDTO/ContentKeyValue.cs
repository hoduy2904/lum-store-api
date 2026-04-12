using System;

namespace LumStoreAPI.Application.DTOs.StoreDTO;

public class ContentKeyValue
{
    public string Key { get; set; } = default!;
    public object? Value { get; set; }
}
