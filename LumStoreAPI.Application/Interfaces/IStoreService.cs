using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IStoreService
    {
        // ── Categories ────────────────────────────────────────────────────────

        /// <summary>Get all product categories with product counts.</summary>
        Task<IEnumerable<StoreCategoryDTO>> GetCategoriesAsync();

        /// <summary>Get a single category by its NodeAlias slug.</summary>
        Task<StoreCategoryDTO?> GetCategoryAsync(string slug);

        // ── Products ──────────────────────────────────────────────────────────

        /// <summary>Get paginated product list with filtering and sorting.</summary>
        Task<IPagedEnumerable<StoreProductDTO>> GetProductsAsync(StoreProductListRequest request);

        /// <summary>Get isBestSeller OR isNew products, up to limit.</summary>
        Task<IEnumerable<StoreProductDTO>> GetFeaturedProductsAsync(int limit = 8);

        /// <summary>Get isNew products only, up to limit.</summary>
        Task<IEnumerable<StoreProductDTO>> GetNewArrivalsAsync(int limit = 4);

        /// <summary>Get a single product by its NodeAlias slug.</summary>
        Task<StoreProductDTO?> GetProductAsync(string slug);

        /// <summary>Get all variants for a product identified by NodeAlias slug.</summary>
        Task<IEnumerable<ProductVariantGetDTO>> GetProductVariantsAsync(string slug);

        // ── Home ──────────────────────────────────────────────────────────────
    }
}
