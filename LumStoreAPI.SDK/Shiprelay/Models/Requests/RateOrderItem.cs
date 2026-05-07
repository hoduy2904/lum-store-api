using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public class RateOrderItem
{
    [JsonPropertyName("quantity")]
    public required int Quantity { get; set; }

    [JsonPropertyName("price")]
    public required decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "USD";

    [JsonPropertyName("product_id")]
    public required int ProductId { get; set; }
}
