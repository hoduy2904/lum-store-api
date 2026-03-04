using System.ComponentModel.DataAnnotations.Schema;

namespace LumStoreAPI.Core.Entities.DocumentEngine
{
    public class DocumentNode
    {
        public int NodeID { get; set; }
        public int? ParentNodeID { get; set; }
        public string NodeAlias { get; set; } = default!;
        public int NodeOrder { get; set; } = 1;
        [NotMapped]
        public string RelativeUrl { get; set; } = string.Empty;

        public virtual ICollection<DocumentPage> Properties { get; set; } = [];
        public virtual DocumentNode? Parent { get; set; }
        public virtual ICollection<DocumentNode> Childrens { get; set; } = [];
        public virtual ICollection<DocumentLinkedNode> AncestorNodes { get; set; } = [];
        public virtual ICollection<DocumentLinkedNode> DescendantNodes { get; set; } = [];
    }
}
