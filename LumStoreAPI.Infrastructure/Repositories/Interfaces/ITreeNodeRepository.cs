using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Models.Systems;
using Microsoft.EntityFrameworkCore.Query;

namespace LumStoreAPI.Infrastructure.Repositories.Interfaces
{
    public interface ITreeNodeRepository
    {
        Task<T?> InsertAsync<T>(T page, DocumentNode parent) where T : DocumentPage;
        Task<IEnumerable<T>> InsertsAsync<T>(T[] pages, DocumentNode? parent = null) where T : DocumentPage;
        Task<bool> UpdateAsync<T>(T page) where T : DocumentPage;
        Task<bool> UpdateAsync<T>(int nodeID, Action<UpdateSettersBuilder<T>> properties) where T : DocumentPage;
        Task<int> UpdatesAsync<T>(Action<UpdateSettersBuilder<T>> properties) where T : DocumentPage;
        Task<DocumentPage> UpdateAsync(string className, int nodeID, Dictionary<string, object?> properties);
        Task<int> DeleteAsync(int nodeID, bool hardDelete = false);
        Task<int> DeletesAsync(int[] nodeIDs, bool hardDelete = false);
        Task<bool> MoveAsync(int nodeID, int? parentId = null, int? nestedNodeID = null);
        Task<string> GetRelativeUrl(int nodeID);
        Task<int> RenameNodeAsync(int nodeID, string name);
        Task<WidgetData<object>[]?> UpdateWidgets(int nodeID, WidgetData<object>[] widgetData);
    }
}
