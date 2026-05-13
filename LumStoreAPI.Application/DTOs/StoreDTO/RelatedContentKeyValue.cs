using System;

namespace LumStoreAPI.Application.DTOs.StoreDTO;

public class RelatedContentKeyValue : ContentKeyValue
{
    public IEnumerable<ContentKeyValue> RelatedData { get; set; } = [];
}
