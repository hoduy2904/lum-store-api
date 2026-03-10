namespace LumStoreAPI.Core.Models.Systems
{
    public class DocumentPageType
    {
        public string Name { get; set; } = default!;
        public string DataType { get; set; } = default!;
        public int? MaxLength { get; set; }
        public bool IsNullable { get; set; }
    }
}
