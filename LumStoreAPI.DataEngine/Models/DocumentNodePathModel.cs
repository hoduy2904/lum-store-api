namespace LumStoreAPI.DataEngine.Models
{
    internal record class DocumentNodePathModel<T>
    {
        public T Node { get; set; } = default!;
        public string Path { get; set; } = string.Empty;
    }
}
