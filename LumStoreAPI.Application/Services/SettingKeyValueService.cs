using System;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.Services;

public class SettingKeyValueService(
    ISettingKeyValueRepository settingKeyValueRepository
) : ISettingKeyValueService
{
    private readonly ISettingKeyValueRepository _settingKeyValueRepository = settingKeyValueRepository;
    public async Task<T?> GetSystemSettingAsync<T>()
    {
        string key = SettingKeyHelper.SystemSettingTypeMapping.FirstOrDefault(x => x.Value == typeof(T)).Key;
        if (string.IsNullOrWhiteSpace(key)) return default;

        var data = await _settingKeyValueRepository.GetSettingKeyAsync(key);
        if (data == null) return default;

        return JsonHelper.Deserialize<T>(data.SettingValue, default);
    }

    public async Task<IEnumerable<object>> GetSystemSettingsAsync(params string[] keys)
    {
        keys = keys.Where(x => x.StartsWith("System.")).ToArray();
        if (keys.Length == 0) return [];
        var dataKeys = await _settingKeyValueRepository.GetSettingKeysAsync(x => keys.Contains(x.SettingCode));
        var systemSettings = dataKeys.Select(x =>
        {
            if (SettingKeyHelper.SystemSettingTypeMapping.TryGetValue(x.SettingCode, out Type? type))
            {
                return JsonHelper.Deserialize(x.SettingValue, null, type);
            }
            return null;
        }).OfType<object>();

        return systemSettings;
    }
}
