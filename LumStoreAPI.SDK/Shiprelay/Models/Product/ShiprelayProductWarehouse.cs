using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Product;

public class ShiprelayProductWarehouse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public int Name { get; set; }
    [JsonPropertyName("stock_count")]
    public int StockCount { get; set; }
    [JsonPropertyName("transit_count")]
    public int TransitCount { get; set; }
}
