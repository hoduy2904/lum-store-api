using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentClientGetDTO : DocumentClientBaseDTO
    {
        [JsonIgnore]
        public IGenericFeatureQuery? FeatureQuery { get; set; }
        public WidgetData<object>[] DocumentPageWidgets { get; set; } = [];
        public Dictionary<string, object?> Fields { get; set; } = [];

        public DocumentClientGetDTO(DocumentPage documentPage) : base(documentPage)
        {
            this.DocumentPageWidgets = documentPage.DocumentPageWidgets;
            if (DocumentPageTypeHelper.DocumentFeatureQueries.TryGetValue(documentPage.GetType(), out var query))
            {
                this.FeatureQuery = (IGenericFeatureQuery?)Activator.CreateInstance(query, documentPage);
            }
            var type = documentPage.GetType();
            foreach (var property in type.GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly
                ).Where(x => x.DeclaringType != typeof(DocumentPage)))
            {
                if (!this.Fields.ContainsKey(property.Name))
                {
                    this.Fields.Add(property.Name.ToCamelCase(), property.GetValue(documentPage));
                }
            }
        }
    }

    public class DocumentClientGetDTO<T> : DocumentClientBaseDTO
    {
        public T Fields { get; set; }
        public DocumentClientGetDTO(T fields, DocumentPage documentPage) : base(documentPage)
        {
            this.Fields = fields;
        }
    }
}
