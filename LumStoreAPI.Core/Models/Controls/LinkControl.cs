namespace LumStoreAPI.Core.Models.Controls
{
    public class LinkControl
    {
        public string Name { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string? Target { get; set; }
    }
}
