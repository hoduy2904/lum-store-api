using System;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Web;

namespace LumStoreAPI.Libraries.Extensions;

public static class ObjectExtensions
{
    extension(object obj)
    {
        public string ToQueryString()
        {
            if (obj == null) return string.Empty;

            var properties = obj.GetType().GetProperties()
                .Select(p =>
                {
                    var value = p.GetValue(obj);
                    if (value == null) return null;

                    var jsonAttribute = p.GetCustomAttribute<JsonPropertyNameAttribute>();

                    string name = jsonAttribute?.Name ?? p.Name;

                    // DateTime/DateTimeOffset must be ISO 8601 ("o") so that API filter params
                    // like updated_at_from are sent as "2026-06-08T12:00:00.0000000Z" rather than
                    // a locale-dependent string (e.g. "06/08/2026 12:00:00") which remote APIs reject.
                    string raw = value switch
                    {
                        DateTime dt       => dt.ToUniversalTime().ToString("o"),
                        DateTimeOffset dto => dto.ToUniversalTime().ToString("o"),
                        _                 => value.ToString() ?? string.Empty
                    };
                    string valueString = HttpUtility.UrlEncode(raw);

                    return $"{name}={valueString}";
                })
                .Where(x => x != null);

            return "?" + string.Join("&", properties);
        }
    }
}
