using System;
using System.Text.Json.Serialization;
using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.DocumentTypes;

namespace LumStoreAPI.Core.Entities.Systems;

public class ColorItem : BaseClassItem
{
    public int ItemOrder { get; set; }
    public string ColorName { get; set; } = default!;
    public string? ColorValue { get; set; }
    public Guid? ColorImageId { get; set; }
    public int CategoryId { get; set; }
    [JsonIgnore]
    public virtual ColorCategory? ColorCategory { get; set; }
    [JsonIgnore]
    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = [];
}
