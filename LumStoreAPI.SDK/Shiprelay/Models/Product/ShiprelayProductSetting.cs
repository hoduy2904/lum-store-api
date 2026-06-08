using System;
using System.Text.Json.Serialization;

namespace LumStoreAPI.SDK.Shiprelay.Models.Product;

public class ShiprelayProductSetting
{
    [JsonPropertyName("ship_width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShipWidth { get; set; }

    [JsonPropertyName("ship_length")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShipLength { get; set; }

    [JsonPropertyName("ship_height")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShipHeight { get; set; }

    [JsonPropertyName("ship_weight")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShipWeight { get; set; }
    [JsonPropertyName("tariff_code")]
    public string? TariffCode { get; set; }
    [JsonPropertyName("parent_qty")]
    public int? ParentQty { get; set; }
    [JsonPropertyName("is_requestable")]
    public bool IsRequestable { get; set; }
    [JsonPropertyName("is_fragile")]
    public bool IsFragile { get; set; }
    [JsonPropertyName("is_foldable")]
    public bool IsFoldable { get; set; }
    [JsonPropertyName("is_alcoholic")]
    public bool IsAlcoholic { get; set; }
    [JsonPropertyName("is_hazmat")]
    public bool IsHazmat { get; set; }
    [JsonPropertyName("needs_box")]
    public bool NeedsBox { get; set; }
    [JsonPropertyName("needs_segregation")]
    public bool NeedsSegregation { get; set; }
    [JsonPropertyName("is_meshable")]
    public bool IsMeshable { get; set; }
    [JsonPropertyName("source_id")]
    public int? SourceId { get; set; }
    [JsonPropertyName("source_unit_cost")]
    public decimal SourceUnitCost { get; set; }
    [JsonPropertyName("min_product_stock_threshold")]
    public int MinProductStockThreshold { get; set; }
    [JsonPropertyName("default_retail_price_per_unit")]
    public decimal DefaultRetailPricePerUnit { get; set; }
}
