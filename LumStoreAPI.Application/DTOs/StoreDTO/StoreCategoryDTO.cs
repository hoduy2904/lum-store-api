namespace LumStoreAPI.Application.DTOs.StoreDTO
{
    /// <summary>
    /// Category DTO for the public storefront — mirrors lum-nails Category type exactly.
    /// </summary>
    public class StoreCategoryDTO
    {
        /// <summary>NodeID as string.</summary>
        public int Id { get; set; } = default!;

        /// <summary>NodeAlias — used as URL slug: /collection/[slug]</summary>
        public string Slug { get; set; } = default!;

        /// <summary>CategoryName display label.</summary>
        public string Name { get; set; } = default!;

        /// <summary>CategoryImage URL (Unsplash or uploaded media).</summary>
        public string? Image { get; set; }

        public string? Description { get; set; }

        /// <summary>Number of published products in this category.</summary>
        public int ProductCount { get; set; }
    }
}
