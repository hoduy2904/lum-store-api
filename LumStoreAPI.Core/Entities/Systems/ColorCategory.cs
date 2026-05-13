using System;
using System.Text.Json.Serialization;
using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.Systems;

public class ColorCategory : BaseClassItem
{
    public string CategoryName { get; set; } = default!;
    public int ItemOrder { get; set; }
    [JsonIgnore]
    public virtual ICollection<ColorItem> Colors { get; set; } = [];
}
