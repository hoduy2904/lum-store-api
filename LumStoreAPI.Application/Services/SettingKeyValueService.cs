using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.Services;

public class SettingKeyValueService(
    ISettingKeyValueRepository settingKeyValueRepository,
    ICacheService cacheService
) : ISettingKeyValueService
{
    private readonly ISettingKeyValueRepository _settingKeyValueRepository = settingKeyValueRepository;
    private readonly ICacheService _cacheService = cacheService;

    public Task<IEnumerable<SettingKeyValue>> GetSettingsAsync(params string[] keys)
    {
        if (keys.Length == 0) return Task.FromResult<IEnumerable<SettingKeyValue>>([]);

        return _cacheService.GetCacheAsync(() =>
            _settingKeyValueRepository.GetSettingKeysAsync(x => keys.Contains(x.SettingCode)
        ), cache => cache.Dependencies(d =>
        {
            foreach (var key in keys)
            {
                d.SettingKey(key);
            }
        }).Expiration(-1).Key($"setting|keys|{string.Join(',', keys)}"))!;
    }

    public async Task<IEnumerable<ContentKeyValue>> GetSettingContentsAsync(string startWith)
    {
        if (string.IsNullOrWhiteSpace(startWith)) return [];
        var settingKeys = await _cacheService.GetCacheAsync(async () =>
        {
            var data = await _settingKeyValueRepository.GetSettingKeysAsync(query => query.SettingCode.StartsWith(startWith));
            return data.Select(x => new ContentKeyValue { Key = x.SettingName, Value = x.SettingValue });
        }, cache => cache.Dependencies(d => d.SettingKeys()).Key("settings|content|startwith|" + startWith).Expiration(-1));

        return settingKeys ?? [];
    }

    public async Task<T?> GetSystemSettingAsync<T>()
    {
        string key = SettingKeyHelper.SystemSettingTypeMapping.FirstOrDefault(x => x.Value == typeof(T)).Key;
        if (string.IsNullOrWhiteSpace(key)) return default;

        var data = await _settingKeyValueRepository.GetSettingKeyAsync(key);
        if (data == null) return default;

        return JsonHelper.Deserialize<T>(data.SettingValue, default);
    }

    public Task<IEnumerable<ContentKeyValue>> GetSystemSettingsAsync(params string[] keys)
    {
        keys = keys.Where(x => x.StartsWith("System.")).ToArray();
        if (keys.Length == 0) return Task.FromResult<IEnumerable<ContentKeyValue>>([]);

        var systemSettings = _cacheService.GetCacheAsync(async () =>
          {
              var dataKeys = await _settingKeyValueRepository.GetSettingKeysAsync(x => keys.Contains(x.SettingCode));
              return dataKeys.Select(x =>
            {
                if (SettingKeyHelper.SystemSettingTypeMapping.TryGetValue(x.SettingCode, out Type? type))
                {
                    return new ContentKeyValue { Key = x.SettingCode, Value = JsonHelper.Deserialize(x.SettingValue, null, type) };
                }
                return null;
            }).OfType<ContentKeyValue>();
          }, cache => cache.Dependencies(d =>
          {
              foreach (var key in keys)
              {
                  d.SettingKey(key);
              }
          }).Expiration(0).Key($"systemkeys|{string.Join('|', keys)}"));


        return systemSettings!;
    }
}
