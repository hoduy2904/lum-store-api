using LumStoreAPI.Core.Entities.Base;

namespace LumStoreAPI.Core.Entities.DocumentEngine
{
    public class DocumentPage : BaseItem
    {
        public int PageID { get; set; }
        public string DocumentName { get; set; } = default!;
        public int NodeID { get; set; }
        public bool RequireAuthentication { get; set; }
        public virtual string ClassName { get; set; } = "CMS.Folder";
        public bool IsDeleted { get; set; }
        public DateTime? PublishedFrom { get; set; }
        public DateTime? PublishedTo { get; set; }

        public virtual DocumentNode Node { get; set; } = default!;

    }
}
