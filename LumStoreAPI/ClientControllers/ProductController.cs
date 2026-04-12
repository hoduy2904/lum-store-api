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
    }
}
