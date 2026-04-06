namespace LumStoreAPI.Application.DTOs.StoreDTO
{
    /// <summary>
    /// Product DTO for the public storefront — mirrors lum-nails Product type exactly.
    /// </summary>
    public class StoreProductDTO
    {
        /// <summary>PageID as string — used as cart item key.</summary>
        public string Id { get; set; } = default!;

        /// <summary>NodeAlias — used as URL slug: /product/[slug]</summary>
        public string Slug { get; set; } = default!;

        /// <summary>ProductName</summary>
        public string Title { get; set; } = default!;

        /// <summary>Primary variant SKU (first variant).</summary>
        public string Sku { get; set; } = default!;

        /// <summary>Actual sale price = Price - PriceDiscount.</summary>
        public decimal Price { get; set; }

        /// <summary>Original price (only set when PriceDiscount > 0).</summary>
        public decimal? OldPrice { get; set; }

        /// <summary>Media URLs for gallery. Empty until images uploaded via admin.</summary>
        public string[] Images { get; set; } = [];

        /// <summary>Parent category display name, e.g. "Gel Polish".</summary>
        public string Category { get; set; } = default!;

        /// <summary>Parent category NodeAlias, e.g. "gel-polish".</summary>
        public string CategorySlug { get; set; } = default!;

        public string[] Tags { get; set; } = [];
        public string? Description { get; set; }
        public string? ShortDescription { get; set; }

        public bool IsNew { get; set; }
        public bool IsBestSeller { get; set; }

        /// <summary>Derived: PriceDiscount > 0.</summary>
        public bool IsSale { get; set; }

        public double Rating { get; set; }
        public int ReviewCount { get; set; }

        /// <summary>Total stock across all variants.</summary>
        public int Stock { get; set; }
    }
}
