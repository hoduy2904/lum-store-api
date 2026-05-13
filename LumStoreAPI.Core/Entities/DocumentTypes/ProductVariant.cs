using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Entities.Systems;

namespace LumStoreAPI.Core.Entities.DocumentTypes
{
    public class ProductVariant : BaseClassItem
    {
        public int ProductID { get; set; }
        public string SKU { get; set; } = default!;
        public string UPC { get; set; } = default!;
        public int Stock { get; set; }
        public Guid[] Images { get; set; } = [];
        public int? ColorId { get; set; }
        public string VariantName { get; set; } = default!;
        public int ShiprelayId { get; set; }

        public virtual Product? Product { get; set; }
        public virtual ColorItem? Color { get; set; }
    }
}
