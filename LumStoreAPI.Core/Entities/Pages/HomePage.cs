using System.ComponentModel;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Pages
{
    [RegisterPageType(HomePage.CLASS_NAME, typeof(HomePage))]
    public class HomePage : DocumentPage
    {
        public const string CLASS_NAME = "Pages.HomePage";

        [DocumentName]
        [DisplayName("Page title")]
        public string PageTitle { get; set; } = default!;

        public string? Description { get; set; }
        public int CarouselPathId { get; set; }
    }
}
