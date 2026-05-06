using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Pages
{
    public class GeneralContent : DocumentPage
    {
        public const string CLASS_NAME = "Pages.GeneralContent";
        [DocumentName]
        public string Title { get; set; } = default!;
        public string? Descrition { get; set; }
        public string? Content { get; set; }
    }
}
