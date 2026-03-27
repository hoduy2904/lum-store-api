using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class DocumentTableService : IDocumentTableService
    {
        private readonly LumStoreContext _lumStoreContext;
        public DocumentTableService(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public DocumentTable? GetSchemaTable(string className)
        {
            if (className.Equals("CMS.Folder", StringComparison.OrdinalIgnoreCase))
            {
                return new DocumentTable
                {
                    ClassName = className,
                    PageTypes = [
                    new DocumentPageType{
                        Name = "DocumentName",
                        DataType = "String",
                        MaxLength = 100,
                        IsNullable = false,
                        DisplayName = "Document Name"
                    }
                ]
                };
            }

            if (!DocumentPageTypeHelper.DocumentPageTypes.ContainsKey(className))
            {
                return null;
            }

            return GetSchemaTables().FirstOrDefault(x => x.ClassName.Equals(className));


        }

        public IEnumerable<DocumentTable> GetSchemaTables()
        {
            var tables = _lumStoreContext.Model.GetEntityTypes()
            .Where(x => x.ClrType.GetCustomAttribute<RegisterPageTypeAttribute>() != null)
                .Select(x => new DocumentTable
                {
                    ClassName = x.ClrType.GetField("CLASS_NAME", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString() ?? "CMS.Folder",
                    PageTypes = x.GetDeclaredProperties()
                    .Reverse()
                    .Union(x.GetPropertiesInHierarchy())
                    .Where(p => p.PropertyInfo?.GetCustomAttribute<JsonIgnoreAttribute>(true) is null)
                   .Select(x => new DocumentPageType
                   {
                       Name = x.Name,
                       DisplayName = x.PropertyInfo?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name,
                       DataType = (Nullable.GetUnderlyingType(x.ClrType) ?? x.ClrType).Name,
                       IsNullable = x.IsNullable,
                       MaxLength = x.GetMaxLength()
                   })
                });

            return tables.Union([new DocumentTable
            {
                ClassName = "CMS.Folder",
                PageTypes = []
            }]);
        }
    }
}
