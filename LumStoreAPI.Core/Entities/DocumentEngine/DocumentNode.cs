using System.ComponentModel.DataAnnotations.Schema;

namespace LumStoreAPI.Core.Entities.DocumentEngine
{
    public class DocumentNode
    {
        public int NodeID { get; set; }
        public string NodeName { get; set; } = default!;
        public int? ParentNodeID { get; set; }
        public string NodeAlias { get; set; } = default!;
        public int NodeOrder { get; set; } = 1;
        public string ClassName { get; set; } = "CMS.Folder";
        public string RelativeUrl { get; set; } = default!;

        public virtual ICollection<DocumentPage> Properties { get; set; } = [];
        public virtual DocumentNode? Parent { get; set; }
        public virtual ICollection<DocumentNode> Childrens { get; set; } = [];
        public virtual ICollection<DocumentLinkedNode> AncestorNodes { get; set; } = [];
        public virtual ICollection<DocumentLinkedNode> DescendantNodes { get; set; } = [];
    }
}
