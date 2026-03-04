namespace LumStoreAPI.Core.Entities.DocumentEngine
{
    public class DocumentLinkedNode
    {
        public int Ancestor { get; set; }
        public int Descendant { get; set; }
        public int Depth { get; set; } = 0;

        public virtual DocumentNode AncestorNode { get; set; } = default!;
        public virtual DocumentNode DescendantNode { get; set; } = default!;
    }
}
