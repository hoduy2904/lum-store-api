using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models;

public class ShiprelayPagedResponse<T>
{
    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; set; } = [];
    [JsonPropertyName("meta")]
    public ShiprelayMetaResponse Meta { get; set; } = default!;
}
