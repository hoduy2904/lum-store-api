using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LumStoreAPI.Libraries.Extensions
{
    public static class StringExtensions
    {
        extension(string input)
        {
            public string Slug
            {
                get
                {
                    string str = input.RemoveAccents().ToLower();
                    str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
                    str = Regex.Replace(str, @"\s+", " ").Trim();
                    str = Regex.Replace(str, @"\s", "-");
                    return str;
                }
            }

            string RemoveAccents()
            {
                if (string.IsNullOrWhiteSpace(input))
                    return input;

                input = input.Normalize(NormalizationForm.FormD);
                char[] chars = input
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray();
                return new string(chars).Normalize(NormalizationForm.FormC);
            }

            public string ToCamelCase()
            {
                return JsonNamingPolicy.CamelCase.ConvertName(input);
            }

            public bool IsValidJson(bool checkWithArray = false)
            {
                if (string.IsNullOrWhiteSpace(input)) return false;
                try
                {
                    using var doc = JsonDocument.Parse(input);
                    if (checkWithArray)
                        return doc.RootElement.ValueKind == JsonValueKind.Array;
                    return doc.RootElement.ValueKind == JsonValueKind.Object;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
