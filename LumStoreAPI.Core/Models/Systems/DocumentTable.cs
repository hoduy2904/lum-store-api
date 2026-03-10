namespace LumStoreAPI.Core.Models.Systems
{
    public class DocumentTable
    {
        public string ClassName { get; set; } = default!;
        public IEnumerable<DocumentPageType> PageTypes { get; set; } = [];
    }
}
