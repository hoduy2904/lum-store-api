using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController
    (
        IProductService productService
    )
     : ControllerBase
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductClientRequestDTO request)
        {
            var products = await _productService.GetProducts(request);
            return Ok(PagedResponse<DocumentClientGetDTO>.Success(products, request.Page, request.PageSize));
        }

        [HttpGet("search-suggestions")]
        public async Task<IActionResult> GetSearchSuggestions([FromQuery] string? q, [FromQuery] int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(APIResponse<IEnumerable<object>>.Success([], ["No query"]));

            var suggestions = await _productService.GetSearchSuggestionsAsync(q, limit);
            return Ok(APIResponse<IEnumerable<SearchSuggestionDTO>>.Success(suggestions, ["Success"]));
        }

        [HttpGet("search-recommendations")]
        public async Task<IActionResult> GetSearchRecommendations([FromQuery] int limit = 9)
        {
            var recommendations = await _productService.GetSearchRecommendationsAsync(limit);
            return Ok(APIResponse<IEnumerable<string>>.Success(recommendations, ["Success"]));
        }

        [HttpGet("{nodeAlias}/related")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRelatedProducts([FromRoute] string nodeAlias, [FromQuery] int limit = 8)
        {
            limit = Math.Clamp(limit, 1, 15);
            var products = await _productService.GetRelatedProductsAsync(nodeAlias, limit);
            return Ok(APIResponse<IEnumerable<DocumentClientGetDTO>>.Success(products));
        }

        [HttpGet("by-color")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductByColor([FromQuery] string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return Ok(APIResponse<ProductByColorDTO>.Failure("color param is required"));

            var product = await _productService.GetProductByColorAsync(color);

            if (product == null)
                return Ok(APIResponse<ProductByColorDTO>.Success(null, [$"No product found for color: {color}"]));

            return Ok(APIResponse<ProductByColorDTO>.Success(product));
        }

        [HttpGet("by-color/products")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductsByColor([FromQuery] string? color, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(color))
                return BadRequest(APIResponse<object>.Failure("color param is required"));

            pageSize = Math.Clamp(pageSize, 1, 50);
            page = Math.Max(page, 1);

            var products = await _productService.GetProductsByColorAsync(color, page, pageSize);

            if (!products.Any())
                return Ok(PagedResponse<ProductByColorItemDTO>.Success(
                    products, page, pageSize,
                    [$"No products found for color: {color}"]));

            return Ok(PagedResponse<ProductByColorItemDTO>.Success(products, page, pageSize));
        }
    }
}
