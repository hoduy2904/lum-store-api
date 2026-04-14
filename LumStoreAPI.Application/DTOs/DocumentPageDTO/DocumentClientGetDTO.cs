using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentClientGetDTO : DocumentClientBaseDTO
    {
        public object? Fields { get; set; }

        public DocumentClientGetDTO(DocumentPage documentPage) : base(documentPage)
        {
            var type = documentPage.GetType();
            var fields = new Dictionary<string, object?>();
            foreach (var property in type.GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.DeclaredOnly
                ).Where(x => x.DeclaringType != typeof(DocumentPage)))
            {
                if (!fields.ContainsKey(property.Name))
                {
                    fields.Add(property.Name.ToCamelCase(), property.GetValue(documentPage));
                }
            }

            this.Fields = fields;
        }

        public DocumentClientGetDTO(object fields, DocumentPage documentPage) : base(documentPage)
        {
            this.Fields = fields;
        }
    }
}
