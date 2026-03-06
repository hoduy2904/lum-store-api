using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Libraries.Helpers;
using System.Reflection;
using System.Text.Json;

namespace LumStoreAPI.Application.DTOs
{
    public class DocumentPageDTO
    {
        public string DocumentName { get; set; } = default!;
        public string ClassName { get; set; } = default!;
        public int? ParentNodeID { get; set; }
        public Dictionary<string, object> Fields { get; set; } = [];

        public DocumentPage GetEntity()
        {
            var type = DocumentPageTypeHelper.DocumentPageTypes.GetValueOrDefault(this.ClassName, typeof(DocumentPage));
            var entity = (DocumentPage)Activator.CreateInstance(type)!;

            foreach (var field in this.Fields)
            {
                var prop = type.GetProperty(field.Key,
        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop != null)
                {
                    object? value = field.Value;

                    if (value is JsonElement json)
                    {
                        value = JsonSerializer.Deserialize(
                            json.GetRawText(),
                            prop.PropertyType
                        );
                    }
                    prop.SetValue(entity, Convert.ChangeType(value, prop.PropertyType));
                }
            }

            return entity;
        }
    }
}
