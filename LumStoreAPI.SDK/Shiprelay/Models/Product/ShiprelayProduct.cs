using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Product;

public class ShiprelayProduct
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("suite_id")]
    public int SuiteId { get; set; }
    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;
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
    [JsonPropertyName("stock_count")]
    public int StockCount { get; set; }
    [JsonPropertyName("reserved_count")]
    public int ReservedCount { get; set; }
    [JsonPropertyName("available_count")]
    public int AvailableCount { get; set; }
    [JsonPropertyName("once_received")]
    public bool OnceReceived { get; set; }
    [JsonPropertyName("settings")]
    public ShiprelayProductSetting Settings { get; set; } = default!;
    [JsonPropertyName("warehouse_counts")]
    public IEnumerable<ShiprelayProductWarehouse> WareHouseCounts { get; set; } = default!;
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
    [JsonPropertyName("archived_at")]
    public DateTime? ArchivedAt { get; set; }
}
