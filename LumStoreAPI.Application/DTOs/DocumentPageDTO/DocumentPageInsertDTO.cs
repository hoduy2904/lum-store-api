using LumStoreAPI.Application;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Libraries.Helpers;
using System.Reflection;
using System.Text.Json;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentPageInsertDTO
    {
        public string DocumentName { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public int? ParentNodeID { get; set; }
        public Dictionary<string, object> Fields { get; set; } = [];

        public DocumentPage GetEntity()
        {
            var type = DocumentPageTypeHelper.DocumentPageTypes.GetValueOrDefault(this.ClassName, typeof(DocumentPage));
            var entity = (DocumentPage)Activator.CreateInstance(type)!;

            entity.ClassName = type.GetField("CLASS_NAME", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString() ?? "CMS.Folder";

            foreach (var field in this.Fields)
            {
                var prop = type.GetProperty(field.Key,
        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop == null && DocumentPageTypeHelper.IsAllowSystemField(field.Key))
                {
                    prop = type.GetProperty(field.Key);
                }
                if (prop != null)
                {
                    object? value = field.Value;
                    if (value is JsonElement json)
                    {
                        if (prop.PropertyType == typeof(DateTime?))
                        {
                            value = JsonSerializer.Deserialize(
                                json.GetDateTime(),
                                prop.PropertyType
                            );
                        }
                        else if (prop.PropertyType == typeof(DateTimeOffset?))
                        {
                            value = JsonSerializer.Deserialize(
                               json.GetDateTimeOffset(),
                               prop.PropertyType
                           );
                        }
                        else
                        {
                            value = JsonSerializer.Deserialize(
                                json.GetRawText(),
                                prop.PropertyType
                            );
                        }
                    }
                    prop.SetValue(entity, value == null ? null : Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                }
            }

            return entity;
        }
    }
}
