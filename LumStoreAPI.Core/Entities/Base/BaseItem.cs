using System.Text.Json.Serialization;

namespace LumStoreAPI.Core.Entities.Base
{
    public class BaseItem
    {
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }
    }
}
