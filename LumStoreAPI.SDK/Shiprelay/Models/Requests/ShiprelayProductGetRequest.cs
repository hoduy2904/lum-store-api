using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public sealed record class ShiprelayProductGetRequest
{
    [JsonPropertyName("page")]
    public int Page { get; set; }
    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("sku")]
    public string? SKU { get; set; }
    [JsonPropertyName("source_id")]
    public int? SourceId { get; set; }
    [JsonPropertyName("updated_at_from")]
    public DateTime? UpdatedAtFrom { get; set; }
    [JsonPropertyName("updated_at_to")]
    public DateTime? UpdatedAtTo { get; set; }
}
