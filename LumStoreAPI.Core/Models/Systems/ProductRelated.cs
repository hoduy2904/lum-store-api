namespace LumStoreAPI.Core.Models.Systems
{
    public class ProductRelated
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = default!;
        public string ProductVariantName { get; set; } = default!;
    }
}
