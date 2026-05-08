using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public class RateRequest
{
    [JsonPropertyName("reseller_id")]
    public string ResellerId { get; internal set; } = default!;

    [JsonPropertyName("destination")]
    public required RateDestination Destination { get; set; }

    [JsonPropertyName("items")]
    public required IEnumerable<RateOrderItem> Items { get; set; }
}
