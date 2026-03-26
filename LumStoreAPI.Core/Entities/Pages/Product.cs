using System.ComponentModel;
using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Core.Entities.Pages
{
    [RegisterPageType(CLASS_NAME, typeof(Product))]
    public class Product : DocumentPage
    {
        public const string CLASS_NAME = "Pages.Product";

        [DocumentName]
        [DisplayName("Product Name")]
        public string ProductName { get; set; } = default!;
        public string? UPC { get; set; }
        public string SKU { get; set; } = default!;
        public string[] Images { get; set; } = [];
        public decimal Price { get; set; }
        [DisplayName("Short Description")]
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
    }
}
