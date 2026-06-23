using System.Net;
using System.Text.Json;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.Systems;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Authorization;
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
            search = search?.Trim();
            var settingKeys = await _settingKeyValueRepository.GetSettingKeysAsync(page, pageSize,
                query => !query.SettingCode.StartsWith("System.") &&
                         (string.IsNullOrWhiteSpace(search) || query.SettingCode.StartsWith(search)));

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
            if (settingKeyValue.SettingCode.StartsWith("System"))
                return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA,
                    ["Cannot use system code name"]));
            var settingKey = await _settingKeyValueRepository.InsertSettingKeyAsync(settingKeyValue);
            if (settingKey == null) return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR));

            return Ok(APIResponse<SettingKeyValue>.Success(settingKey));
        }

        [HttpPut("{settingCode}")]
        [ProducesResponseType<APIResponse<SettingKeyValue>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PutSettingKey(string settingCode, SettingKeyValue settingKeyValue)
        {
            if (settingKeyValue.SettingCode.StartsWith("System"))
                return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA,
                    ["Cannot use system code name"]));
            var settingKey = await _settingKeyValueRepository.UpdateSettingKeyAsync(settingCode, settingKeyValue);
            if (settingKey == null) return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR));

            return Ok(APIResponse<SettingKeyValue>.Success(settingKey));
        }

        [HttpPut("bysystem")]
        [ProducesResponseType<APIResponse<SettingKeySystemRequest>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> PutSystemSettingKey(SettingKeySystemRequest request)
        {
            if (!SettingKeyHelper.SystemSettingTypeMapping.TryGetValue(request.SettingCode, out Type? systemType))
            {
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            }

            try
            {
                var obj = request.SettingValue.Deserialize(systemType, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                var objJson = JsonSerializer.Serialize(obj);

                var settingEntity = new SettingKeyValue
                {
                    SettingCode = request.SettingCode,
                    SettingName = request.SettingName,
                    SettingValue = objJson
                };
                var updated =
                    await _settingKeyValueRepository.UpdateSettingKeyAsync(request.SettingCode, settingEntity);

                if (updated is null)
                {
                    await _settingKeyValueRepository.InsertSettingKeyAsync(settingEntity);
                }

                return Ok(APIResponse<object>.Success(obj!));
            }
            catch
            {
                return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Please try later"]));
            }
        }

        [HttpGet("{key}/bysystem")]
        [ProducesResponseType<APIResponse<SettingKeySystemRequest>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType<APIResponseBase>((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetSystemKeySetting(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || !key.StartsWith("System"))
                return BadRequest(APIResponseBase.Failure(ErrorStatusNameConstants.INVALID_DATA));
            if (!SettingKeyHelper.SystemSettingTypeMapping.TryGetValue(key, out Type? settingKeyType))
                return NotFound();
            var model = new SettingKeySystemResponse();
            var settingKey = await _settingKeyValueRepository.GetSettingKeyAsync(key);

            model.SettingName = settingKey?.SettingName ?? "";
            model.SettingCode = settingKey?.SettingCode ?? key;
            model.SettingValue = JsonHelper.Deserialize(settingKey?.SettingValue, null, settingKeyType);

            return Ok(APIResponse<SettingKeySystemResponse>.Success(model));
        }
    }
}