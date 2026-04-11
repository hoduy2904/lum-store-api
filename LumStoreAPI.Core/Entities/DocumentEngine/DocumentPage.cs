using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Systems;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
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
        [DisplayName("Required authentication")]
        public bool RequireAuthentication { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; }
        [DisplayName("Published to")]
        public DateTimeOffset? PublishedTo { get; set; }
        [DisplayName("Published from")]
        public DateTimeOffset? PublishedFrom { get; set; }
        [NotMapped]
        public bool IsPublished => (this.PublishedFrom == null || this.PublishedFrom <= DateTime.UtcNow) && (this.PublishedTo == null || this.PublishedTo > DateTime.UtcNow);
        public WidgetData<object>[] DocumentPageWidgets { get; set; } = [];

        public virtual DocumentNode Node { get; set; } = default!;

    }
}
