namespace LumStoreAPI.Core.Models.Systems
{
    public class ComboItemPricing
    {
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string VariantName { get; set; } = default!;
        public int SubProductNodeId { get; set; }
        public string SubProductName { get; set; } = default!;
        public decimal SubProductPrice { get; set; }
        public decimal SubProductPriceDiscount { get; set; }
        public int Stock { get; set; }
        public int ShiprelayId { get; set; }
    }
}
