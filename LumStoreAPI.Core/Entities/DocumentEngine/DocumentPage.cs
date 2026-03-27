using LumStoreAPI.Core.Entities.Base;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Core.Entities.DocumentEngine
{
    public class DocumentPage : BaseItem
    {
        [JsonIgnore]
        public int PageID { get; set; }
        [JsonIgnore]
        public string DocumentName { get; set; } = default!;
        [JsonIgnore]
        public int NodeID { get; set; }
        public bool RequireAuthentication { get; set; }
        [JsonIgnore]
        public virtual string ClassName { get; set; } = "CMS.Folder";
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        public DateTimeOffset? PublishedTo { get; set; }
        public DateTimeOffset? PublishedFrom { get; set; }

        public virtual DocumentNode Node { get; set; } = default!;

    }
}
