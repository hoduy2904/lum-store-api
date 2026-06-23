using LumStoreAPI.Application.DTOs.ColorDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = nameof(RoleType.MANAGER_TYPE))]
    public class ColorCategoryController(
        IColorCategoryRepository colorCategoryRepository
    ) : ControllerBase
    {
        private readonly IColorCategoryRepository _colorCategoryRepository = colorCategoryRepository;

        [HttpGet]
        public async Task<IActionResult> GetCategories(int page, int pageSize, string query = "")
        {
            var categories = await _colorCategoryRepository.GetColorCategoriesAsync(page, pageSize, query);
            return Ok(PagedResponse<ColorCategory>.Success(categories, page, pageSize));
        }

        [HttpGet("{categoryId:int}")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            var category = await _colorCategoryRepository.GetCategoryAsync(categoryId);
            if (category == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Not found"]));

            return Ok(APIResponse<ColorCategory>.Success(category));
        }

        [HttpPost]
        public async Task<IActionResult> PostCategory(ColorCategoryInsertRequestDTO request)
        {
            var category = await _colorCategoryRepository.InsertCategoryAsync(request.CategoryName);

            return Ok(APIResponse<ColorCategory>.Success(category, ["Created"]));
        }

        [HttpPut("{categoryId:int}")]
        public async Task<IActionResult> PutCategory(int categoryId, ColorCategoryInsertRequestDTO request)
        {
            var category = await _colorCategoryRepository.UpdateCategoryAsync(categoryId,
            x => x.SetProperty(p => p.CategoryName, request.CategoryName));

            return Ok(APIResponseBase.Success(["Updated"]));
        }

        [HttpDelete("{categoryId:int}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var isSuccess = await _colorCategoryRepository.DeleteCategoryAsync(categoryId);
            if (!isSuccess) return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA));
            return Ok(APIResponseBase.Success(["Updated"]));
        }

        [HttpPatch("{categoryId:int}")]
        public async Task<IActionResult> ReOrderPosition(int categoryId, int newPosition)
        {
            var isSuccess = await _colorCategoryRepository.MoveToPositionAsync(categoryId, newPosition);
            if (isSuccess) return Ok(APIResponseBase.Success(["Reoreded"]));
            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Please try later"]));
        }
    }
}
