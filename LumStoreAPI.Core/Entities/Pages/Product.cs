using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel;

namespace LumStoreAPI.Core.Entities.Pages
{
    [RegisterPageType(CLASS_NAME, typeof(Product))]
    public class Product : DocumentPage
    {
        public const string CLASS_NAME = "Pages.Product";

        public bool IsCombo { get; set; }
        public ProductType ProductType { get; set; } = ProductType.SIMPLE;
        public ProductGroup ProductGroup { get; set; } = ProductGroup.COMODITY;

        [DocumentName]
        [DisplayName("Product name")]
        public string ProductName { get; set; } = default!;

        public Guid[] Images { get; set; } = [];

        [DisplayName("Short description")]
        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public decimal PriceDiscount { get; set; }

        [DisplayName("Best seller")]
        public bool IsBestSeller { get; set; }

        public string[] Tags { get; set; } = [];

        [DisplayName("Rating")]
        public double Rating { get; set; }

        [DisplayName("Review count")]
        public int ReviewCount { get; set; }

        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public bool IsFoldable { get; set; }
        public bool IsAlcoholic { get; set; }
        public bool IsHazmat { get; set; }
        public bool IsNeedBox { get; set; }
        public bool IsFragile { get; set; }
        public virtual ICollection<ProductVariant> ProductVariants { get; set; } = [];
        public virtual ICollection<ProductCombo> ProductCombos { get; set; } = [];
    }
}
