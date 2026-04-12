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

                    string valueString = HttpUtility.UrlEncode(value.ToString() ?? string.Empty);

                    return $"{name}={valueString}";
                })
                .Where(x => x != null);

            return "?" + string.Join("&", properties);
        }
    }
}
