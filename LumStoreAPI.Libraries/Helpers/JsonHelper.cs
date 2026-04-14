using LumStoreAPI.Libraries.Extensions;
using System.Text.Json;

namespace LumStoreAPI.Libraries.Helpers
{
    public class JsonHelper
    {
        public static T? Deserialize<T>(string? data, T? defaultValue, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            if (!data.IsValidJson())
            {
                return defaultValue;
            }
            try
            {
                return JsonSerializer.Deserialize<T>(data, jsonSerializerOptions ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return defaultValue;
            }
        }

        public static object? Deserialize(string? data, object? defaultValue, Type type, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            if (!data.IsValidJson())
            {
                return defaultValue;
            }
            try
            {
                return JsonSerializer.Deserialize(data, type, jsonSerializerOptions ?? new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
