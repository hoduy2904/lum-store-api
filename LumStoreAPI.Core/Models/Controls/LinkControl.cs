namespace LumStoreAPI.Core.Models.Controls
{
    public class LinkControl
    {
        public string LinkName { get; set; } = default!;
        public string LinkUrl { get; set; } = default!;
        public string? Target { get; set; }
    }
}
