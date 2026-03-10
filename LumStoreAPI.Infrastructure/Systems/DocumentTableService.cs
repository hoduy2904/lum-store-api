using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;
using System.Reflection;

namespace LumStoreAPI.Infrastructure.Systems
{
    internal class DocumentTableService : IDocumentPageTypeService
    {
        private readonly LumStoreContext _lumStoreContext;
        public DocumentTableService(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public DocumentTable? GetSchemaTable(string className)
        {
            var dataTypes = _lumStoreContext.Model
                  .FindEntityType(DocumentPageTypeHelper.DocumentPageTypes[className])?
                  .GetProperties()
                  .Select(x => new DocumentPageType
                  {
                      Name = x.Name,
                      DataType = x.ClrType.Name,
                      IsNullable = x.IsNullable,
                      MaxLength = x.GetMaxLength()
                  });

            if (dataTypes == null)
                return null;

            return new DocumentTable
            {
                ClassName = className,
                PageTypes = dataTypes
            };
        }

        public IEnumerable<DocumentTable> GetSchemaTables()
        {
            var tables = _lumStoreContext.Model.GetEntityTypes()
                .Select(x => new DocumentTable
                {
                    ClassName = x.GetType().GetField("CLASS_NAME", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString() ?? "CMS.Folder",
                    PageTypes = x.GetProperties().Select(x => new DocumentPageType
                    {
                        Name = x.Name,
                        DataType = x.ClrType.Name,
                        IsNullable = x.IsNullable,
                        MaxLength = x.GetMaxLength()
                    })
                });

            return tables;
        }
    }
}
