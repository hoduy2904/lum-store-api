using System.Text.Json.Serialization;

namespace LumStoreAPI.Core.Entities.Base
{
    public class BaseItem
    {
        [JsonIgnore]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonIgnore]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
