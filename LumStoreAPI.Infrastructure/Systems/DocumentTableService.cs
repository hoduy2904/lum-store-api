using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Extensions;
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

            if (!DocumentPageTypeHelper.DocumentPageTypes.ContainsKey(className) && !className.Equals("CMS.Folder", StringComparison.OrdinalIgnoreCase))
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
                       Name = x.Name.ToCamelCase(),
                       DisplayName = x.PropertyInfo?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name,
                       DataType = (Nullable.GetUnderlyingType(x.ClrType) ?? x.ClrType).Name,
                       IsNullable = x.IsNullable,
                       MaxLength = x.GetMaxLength()
                   })
                });

            return tables.Union([new DocumentTable
            {
                ClassName = "CMS.Folder",
                PageTypes = new List<DocumentPageType>(){
                    new DocumentPageType{
                    Name = nameof(DocumentPage.DocumentName).ToCamelCase(),
                    DataType = "String",
                    DisplayName = "Folder Name",
                    MaxLength = 100
                    }
                }.Union(_lumStoreContext.Model.FindEntityType(typeof(DocumentPage))?
                .GetDeclaredProperties()
                .Where(p => p.PropertyInfo?.GetCustomAttribute<JsonIgnoreAttribute>(true) is null)
                .Select(x => new DocumentPageType
                   {
                       Name = x.Name.ToCamelCase(),
                       DisplayName = x.PropertyInfo?.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? x.Name,
                       DataType = (Nullable.GetUnderlyingType(x.ClrType) ?? x.ClrType).Name,
                       IsNullable = x.IsNullable,
                       MaxLength = x.GetMaxLength()
                   }) ?? [])
            }]);
        }
    }
}
