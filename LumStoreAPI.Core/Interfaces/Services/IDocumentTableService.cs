using LumStoreAPI.Core.Models.Systems;

namespace LumStoreAPI.Core.Interfaces.Services
{
    public interface IDocumentTableService
    {
        DocumentTable? GetSchemaTable(string className);
        IEnumerable<DocumentTable> GetSchemaTables();
    }
}
