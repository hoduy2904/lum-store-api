using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Pages;

namespace LumStoreAPI.Core.Entities.DocumentTypes
{
    public class ProductVariant : BaseClassItem
    {
        public int ProductID { get; set; }
        public string SKU { get; set; } = default!;
        public string UPC { get; set; } = default!;
        public int Stock { get; set; }
        public Guid[] Images { get; set; } = [];
        public string? Color { get; set; }
        public string VariantName { get; set; } = default!;
        public int ShiprelayId { get; set; }

        public virtual Product? Product { get; set; }
    }
}
