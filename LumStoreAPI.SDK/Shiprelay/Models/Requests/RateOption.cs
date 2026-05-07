using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public class RateOption
{
    [JsonPropertyName("service_name")]
    public string ServiceName { get; set; } = default!;

    [JsonPropertyName("service_code")]
    public string ServiceCode { get; set; } = default!;

    [JsonPropertyName("total_price")]
    public decimal TotalPrice { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = default!;

    [JsonPropertyName("min_delivery_date")]
    public DateTime MinDeliveryDate { get; set; }

    [JsonPropertyName("max_delivery_date")]
    public DateTime MaxDeliveryDate { get; set; }

    [JsonPropertyName("phone_required")]
    public bool PhoneRequired { get; set; }
}
