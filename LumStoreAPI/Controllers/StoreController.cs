using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.StoreDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Constants.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    /// <summary>
    /// Public storefront API — all endpoints are anonymous (no auth required).
    /// Consumed by lum-nails (Next.js customer website).
    /// </summary>
    [Route("api/store")]
    [ApiController]
    [AllowAnonymous]
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // ── Categories ────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/store/categories
        /// Returns all product categories with product counts.
        /// Used by: home page collection grid, navigation menus.
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _storeService.GetCategoriesAsync();
            return Ok(APIResponse<IEnumerable<StoreCategoryDTO>>.Success(categories, ["Success"]));
        }

        /// <summary>
        /// GET /api/store/categories/{slug}
        /// Returns a single category by NodeAlias.
        /// Used by: collection page header.
        /// </summary>
        [HttpGet("categories/{slug}")]
        public async Task<IActionResult> GetCategory(string slug)
        {
            var category = await _storeService.GetCategoryAsync(slug);
            if (category == null)
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND,
                    [$"Category '{slug}' not found."]));

            return Ok(APIResponse<StoreCategoryDTO>.Success(category, ["Success"]));
        }

        // ── Products ──────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/store/products
        /// Paginated product list with optional filters.
        /// Query params: page, pageSize, search, category, isNew, isBestSeller,
        ///               isSale, minPrice, maxPrice, sortBy
        /// Used by: /shop, /collection/[slug], /search, /new, /sale
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] StoreProductListRequest request)
        {
            var products = await _storeService.GetProductsAsync(request);
            return Ok(PagedResponse<StoreProductDTO>.Success(products, request.Page, request.PageSize, ["Success"]));
        }

        /// <summary>
        /// GET /api/store/products/featured?limit=8
        /// Products where isBestSeller=true OR isNew=true.
        /// Used by: home page FeaturedCollection section.
        /// </summary>
        [HttpGet("products/featured")]
        public async Task<IActionResult> GetFeaturedProducts([FromQuery] int limit = 8)
        {
            var products = await _storeService.GetFeaturedProductsAsync(limit);
            return Ok(APIResponse<IEnumerable<StoreProductDTO>>.Success(products, ["Success"]));
        }

        /// <summary>
        /// GET /api/store/products/new-arrivals?limit=4
        /// Products where isNew=true.
        /// Used by: home page NostalgicSection.
        /// </summary>
        [HttpGet("products/new-arrivals")]
        public async Task<IActionResult> GetNewArrivals([FromQuery] int limit = 4)
        {
            var products = await _storeService.GetNewArrivalsAsync(limit);
            return Ok(APIResponse<IEnumerable<StoreProductDTO>>.Success(products, ["Success"]));
        }

        /// <summary>
        /// GET /api/store/products/{slug}
        /// Single product detail by NodeAlias slug.
        /// Used by: /product/[slug] page.
        /// </summary>
        [HttpGet("products/{slug}")]
        public async Task<IActionResult> GetProduct(string slug)
        {
            var product = await _storeService.GetProductAsync(slug);
            if (product == null)
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND,
                    [$"Product '{slug}' not found."]));

            return Ok(APIResponse<StoreProductDTO>.Success(product, ["Success"]));
        }

        /// <summary>
        /// GET /api/store/products/{slug}/variants
        /// All variants for a product.
        /// Used by: /product/[slug] color/size selector.
        /// </summary>
        [HttpGet("products/{slug}/variants")]
        public async Task<IActionResult> GetProductVariants(string slug)
        {
            var variants = await _storeService.GetProductVariantsAsync(slug);
            return Ok(APIResponse<IEnumerable<ProductVariantGetDTO>>.Success(variants, ["Success"]));
        }

        // ── Home ──────────────────────────────────────────────────────────────

        /// <summary>
        /// GET /api/store/hero-slides
        /// Hero slides from the HomePage.HeroSlidesJson field.
        /// Used by: home page HeroSlider component.
        /// </summary>
        [HttpGet("hero-slides")]
        public async Task<IActionResult> GetHeroSlides()
        {
            var slides = await _storeService.GetHeroSlidesAsync();
            return Ok(APIResponse<IEnumerable<StoreHeroSlideDTO>>.Success(slides, ["Success"]));
        }
    }
}
