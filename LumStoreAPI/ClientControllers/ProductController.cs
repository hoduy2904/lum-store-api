using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
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
    }
}
