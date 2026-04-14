using LumStoreAPI.Application.DTOs.ProductVariantDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class ProductVariantController : ControllerBase
    {
        private readonly IProductVariantService _productVariantService;
        public ProductVariantController(IProductVariantService productVariantService)
        {
            _productVariantService = productVariantService;
        }

        [AllowAnonymous]
        [HttpGet("byid/{variantId}")]
        public async Task<IActionResult> GetProductVariant(int variantId)
        {
            var productVariant = await _productVariantService.GetProductVariantAsync(variantId);
            if (productVariant == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            return Ok(APIResponse<ProductVariantGetDTO>.Success(productVariant, ["Success"]));
        }

        [HttpGet("bysku/{sku}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductVariant(string sku)
        {
            var productVariant = await _productVariantService.GetProductVariantAsync(sku);
            if (productVariant == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            return Ok(APIResponse<ProductVariantGetDTO>.Success(productVariant, ["Success"]));
        }

        [HttpGet("{productId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductVariants(int productId)
        {
            var productVariants = await _productVariantService.GetProductVariantsAsync(productId);
            return Ok(APIResponse<IEnumerable<ProductVariantGetDTO>>.Success(productVariants, ["Success"]));
        }

        [HttpPost]
        public async Task<IActionResult> InsertProductVariant(ProductVariantRequestDTO requestDTO)
        {
            var result = await _productVariantService.InsertProductVariantAsync(requestDTO);
            return Ok(APIResponse<ProductVariantGetDTO>.Success(result));
        }

        [HttpPut("{variantId}")]
        public async Task<IActionResult> UpdateProductVariant(int variantId, ProductVariantUpdateDTO request)
        {
            var result = await _productVariantService.UpdateProductVariantAsync(variantId, request);
            if (result > 0) return Ok(APIResponseBase.Success(["Update success"]));

            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Please try later"]));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProductVariant([FromBody] int[] variantIds)
        {
            var result = await _productVariantService.DeleteProductVariantsAsync(variantIds);
            if (result <= 0) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            return Ok(APIResponse<int>.Success(result));
        }
    }
}
