using LumStoreAPI.Application.DTOs.ColorDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = nameof(RoleType.MANAGER_TYPE))]
    public class ColorController(
        IColorItemRepository colorItemRepository
    ) : ControllerBase
    {
        private readonly IColorItemRepository _colorItemRepository = colorItemRepository;
        [HttpGet]
        public async Task<IActionResult> GetColors(int page, int pageSize, string q = "")
        {
            var colors = await _colorItemRepository.GetColorsAsync(page, pageSize, query =>
            string.IsNullOrWhiteSpace(q) || query.ColorName.Contains(q));
            return Ok(PagedResponse<ColorItem>.Success(colors, page, pageSize));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetColors()
        {
            var colors = await _colorItemRepository.GetColors().AsNoTracking().ToArrayAsync();
            return Ok(APIResponse<ColorItem[]>.Success(colors ?? []));
        }


        [HttpGet("{parentId:int}/parent")]
        public async Task<IActionResult> GetColors(int parentId, int page, int pageSize, string q = "")
        {
            var colors = await _colorItemRepository.GetColorsAsync(page, pageSize, query =>
            query.CategoryId == parentId && (string.IsNullOrWhiteSpace(q) || query.ColorName.Contains(q)));
            return Ok(PagedResponse<ColorItem>.Success(colors, page, pageSize));
        }

        [HttpGet("{categoryId:int}")]
        public async Task<IActionResult> GetColor(int colorId)
        {
            var color = await _colorItemRepository.GetColorAsync(colorId);
            if (color == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Not found"]));

            return Ok(APIResponse<ColorItem>.Success(color));
        }

        [HttpPost]
        public async Task<IActionResult> PostColor(ColorInsertRequest request)
        {
            var color = await
                _colorItemRepository.InsertColorAsync(request.GetEntity());

            return Ok(APIResponse<ColorItem>.Success(color, ["Created"]));
        }

        [HttpPut("{colorId:int}")]
        public async Task<IActionResult> PutColor(int colorId, ColorInsertRequest request)
        {
            var category = await _colorItemRepository.UpdateColorAsync(colorId,
            x => x.SetProperty(p => p.ColorName, request.ColorName)
            .SetProperty(p => p.ColorValue, request.ColorValue)
            .SetProperty(p => p.ColorImageId, request.ColorImageId));

            return Ok(APIResponseBase.Success(["Updated"]));
        }

        [HttpDelete("{colorId:int}")]
        public async Task<IActionResult> DeleteColor(int colorId)
        {
            var isSuccess = await _colorItemRepository.DeleteColorAsync(colorId);
            if (!isSuccess) return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA));
            return Ok(APIResponseBase.Success(["Updated"]));
        }

        [HttpPatch("{colorId:int}")]
        public async Task<IActionResult> ReOrderPosition(int colorId, int newPosition)
        {
            var isSuccess = await _colorItemRepository.MoveToPositionAsync(colorId, newPosition);
            if (isSuccess) return Ok(APIResponseBase.Success(["Reoreded"]));
            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Please try later"]));
        }
    }
}
