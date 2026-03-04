using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities
{
    public class HomePage : DocumentPage
    {
        public string PageTitle { get; set; } = default!;
        public string? Description { get; set; }
    }
}
