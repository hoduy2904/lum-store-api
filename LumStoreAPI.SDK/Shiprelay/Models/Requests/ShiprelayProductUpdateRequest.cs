using System;
using System.Text.Json.Serialization;
using LumStoreAPI.SDK.Shiprelay.Models.Product;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public class ShiprelayProductUpdateRequest
{
    [JsonPropertyName("parent_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ParentId { get; set; }

    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = default!;

    [JsonPropertyName("category")]
    public string Category { get; set; } = default!;

    [JsonPropertyName("barcode")]
    public string Barcode { get; set; } = default!;

    [JsonPropertyName("sku")]
    public string SKU { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("thumb")]
    public string? Thumb { get; set; }

    [JsonPropertyName("settings")]
    public ShiprelayProductSetting Settings { get; set; } = default!;
}
