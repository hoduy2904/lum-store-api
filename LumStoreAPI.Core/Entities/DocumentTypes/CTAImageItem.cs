using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Models.Controls;

namespace LumStoreAPI.Core.Entities.DocumentTypes
{
    public class CTAImageItem : DocumentPage
    {
        public string Type { get; set; } = "full";
        public const string CLASS_NAME = "Item.CTAImage";
        public string? Pretitle { get; set; }
        [DocumentName]
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public Guid[] Image { get; set; } = [];
        public LinkControl? PrimaryButton { get; set; }
    }
}
