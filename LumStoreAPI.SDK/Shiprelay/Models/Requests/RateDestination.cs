using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public class RateDestination
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("company")]
    public string Company { get; set; } = string.Empty;

    [JsonPropertyName("address1")]
    public string Address1 { get; set; } = string.Empty;

    [JsonPropertyName("address2")]
    public string? Address2 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; } = default!;

    [JsonPropertyName("region")]
    public string Region { get; set; } = default!;

    [JsonPropertyName("country")]
    public string Country { get; set; } = default!;

    [JsonPropertyName("zip")]
    public string Zip { get; set; } = default!;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = default!;

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
