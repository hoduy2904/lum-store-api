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
        public Guid[] Images { get; set; } = [];
        public string? SKU { get; set; }
        public string? UPC { get; set; }
        public string? Color { get; set; }
        public Guid? ColorImageId { get; set; }
        public int? ColorId { get; set; }
        public int? ParentId { get; set; }
    }
}
