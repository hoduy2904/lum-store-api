using LumStoreAPI.Application.DTOs.ProductComboDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = nameof(RoleType.EDITOR_TYPE))]
    public class ProductComboController(
        IProductComboRepository productComboRepository) : ControllerBase
    {
        private readonly IProductComboRepository _productComboRepository = productComboRepository;
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetCombos(int productId)
        {
            var combos = await _productComboRepository.GetProductRelatedsAsync(productId);
            return Ok(APIResponse<IEnumerable<ProductRelated>>.Success(combos));
        }

        [HttpPost]
        public async Task<IActionResult> InsertProductCombo(ProductComboRequest request)
        {
            var combo = await _productComboRepository.InsertProductCombo(new ProductCombo
            {
                ProductID = request.ProductId,
                VariantID = request.VariantId
            });

            return Ok(APIResponse<ProductCombo>.Success(combo));
        }
        [HttpDelete]

        public async Task<IActionResult> DeleteProductCombo(IEnumerable<ProductComboRequest> requests)
        {
            var comboRequests = requests.Select(x => new ProductCombo
            {
                ProductID = x.ProductId,
                VariantID = x.VariantId
            }).ToArray();
            var isSuccess = await _productComboRepository.DeleteProductCombos(comboRequests);
            if (isSuccess) return Ok(APIResponseBase.Success(["Deleted"]));
            return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
        }
    }
}
