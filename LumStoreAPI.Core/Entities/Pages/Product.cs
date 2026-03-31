using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel;

namespace LumStoreAPI.Core.Entities.Pages
{
    [RegisterPageType(CLASS_NAME, typeof(Product))]
    public class Product : DocumentPage
    {
        public const string CLASS_NAME = "Pages.Product";

        public ProductType ProductType { get; set; } = ProductType.SIMPLE;
        public ProductGroup ProductGroup { get; set; } = ProductGroup.COMODITY;
        [DocumentName]
        [DisplayName("Product name")]
        public string ProductName { get; set; } = default!;
        public Guid[] Images { get; set; } = [];
        [DisplayName("Short description")]
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public bool IsFoldable { get; set; }
        public bool IsAlcoholic { get; set; }
        public bool IsHazmat { get; set; }
        public bool IsNeedBox { get; set; }
        public bool IsFragile { get; set; }
        public int ShiprelayID { get; set; }

        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = [];
    }
}
