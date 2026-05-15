using LumStoreAPI.Core.Entities.Pages;

namespace LumStoreAPI.Core.Entities.DocumentTypes
{
    public class ProductCombo
    {
        public int ProductID { get; set; }
        public int VariantID { get; set; }

        public Product Product { get; set; } = default!;
        public ProductVariant ProductVariant { get; set; } = default!;
    }
}
