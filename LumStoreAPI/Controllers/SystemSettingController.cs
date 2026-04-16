using System.Net;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class SystemSettingController(
        ISettingKeyValueRepository settingKeyValueRepository
    ) : ControllerBase
    {
        private readonly ISettingKeyValueRepository _settingKeyValueRepository = settingKeyValueRepository;

        [HttpGet("{key}")]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.NotFound)]
        [ProducesResponseType<APIResponse<SettingKeyValue>>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSetting(string key)
        {
            if (key.StartsWith("System.")) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            var keyData = await _settingKeyValueRepository.GetSettingKeyAsync(key);
            if (keyData == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));

            return Ok(APIResponse<SettingKeyValue>.Success(keyData));
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings(int page, int pageSize, string? search = null)
        {
            var settingKeys = await _settingKeyValueRepository.GetSettingKeysAsync(page, pageSize,
             query => string.IsNullOrWhiteSpace(search) || query.SettingCode.StartsWith(search));

            return Ok(PagedResponse<SettingKeyValue>.Success(settingKeys, page, pageSize));
        }

        [HttpDelete("{key}")]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.NotFound)]
        [ProducesResponseType<APIResponse<int>>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteSettingKey(string key)
        {
            if (key.StartsWith("System.")) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            var result = await _settingKeyValueRepository.DeleteSettingKeyAsync(key);
            if (result > 0) return Ok(APIResponse<int>.Success(result, ["Deleted"]));
            return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
        }

        [HttpPost]
        [ProducesResponseType<APIResponse<SettingKeyValue>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.InternalServerError)]

        public async Task<IActionResult> PostSettingKey(SettingKeyValue settingKeyValue)
        {
            if (settingKeyValue.SettingCode.StartsWith("System")) return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA, ["Cannot use system code name"]));
            var settingKey = await _settingKeyValueRepository.InsertSettingKeyAsync(settingKeyValue);
            if (settingKey == null) return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR));

            return Ok(APIResponse<SettingKeyValue>.Success(settingKey));
        }

        [HttpPut]
        [ProducesResponseType<APIResponse<SettingKeyValue>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PutSettingKey(SettingKeyValue settingKeyValue)
        {
            if (settingKeyValue.SettingCode.StartsWith("System")) return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA, ["Cannot use system code name"]));
            var settingKey = await _settingKeyValueRepository.UpdateSettingKeyAsync(settingKeyValue);
            if (settingKey == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR));

            return Ok(APIResponse<SettingKeyValue>.Success(settingKey));
        }
    }
}
