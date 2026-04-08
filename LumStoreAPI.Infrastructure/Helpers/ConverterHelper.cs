using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace LumStoreAPI.Infrastructure.Helpers
{
    public class ConverterHelper
    {
        public static ValueConverter<T?, string> ContentConverters<T>(T? defaultValue)
        {
            return new ValueConverter<T?, string>(
                v => JsonSerializer.Serialize(v),
                v => JsonHelper.Deserialize(v, defaultValue, new() { PropertyNameCaseInsensitive = true })
            );
        }

        public static ValueConverter<Guid[], string> ArrayGuidConverter(char split = '|')
        {
            return new ValueConverter<Guid[], string>(x => string.Join(split, x),
                     x => x.Split(split, StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToArray());
        }
    }
}
