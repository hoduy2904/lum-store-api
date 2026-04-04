using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Helpers;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using System.Text.Json;

namespace LumStoreAPI.Infrastructure.Repositories.Presentations
{
    internal class TreeNodeRepository : ITreeNodeRepository
    {
        private readonly ICacheService _cacheService;
        private readonly LumStoreContext _lumStoreContext;
        public TreeNodeRepository(LumStoreContext lumStoreContext, ICacheService cacheService)
        {
            _lumStoreContext = lumStoreContext;
            _cacheService = cacheService;
        }
        public Task<int> DeleteAsync(int nodeID, bool hardDelete = false)
        {
            return DeletesAsync([nodeID], hardDelete);
        }

        public async Task<int> DeletesAsync(int[] nodeIDs, bool hardDelete = false)
        {
            int result = 0;
            if (nodeIDs.Length == 0)
                return 0;
            if (hardDelete)
            {
                using var tx = await _lumStoreContext.Database.BeginTransactionAsync();
                try
                {
                    var childrenNodes = _lumStoreContext.DocumentLinkedNodes
                    .Where(x => nodeIDs.Contains(x.Ancestor))
                    .Select(x => x.Descendant)
                    .ToArray();

                    await _lumStoreContext.DocumentLinkedNodes
                       .Where(x => _lumStoreContext.DocumentLinkedNodes
                                                   .Where(s => nodeIDs.Length == 1 ? s.Ancestor == nodeIDs[0] : nodeIDs.Contains(s.Ancestor))
                                                   .Select(s => s.Descendant)
                                                   .Contains(x.Descendant))
                       .ExecuteDeleteAsync();

                    await _lumStoreContext.DocumentNodes
                    .Where(x => childrenNodes
                            .Any(s => s == x.NodeID))
                            .ExecuteUpdateAsync(x => x.SetProperty(p => p.ParentNodeID, (int?)null));



                    result = await _lumStoreContext.DocumentNodes
                                                      .Where(x => childrenNodes.Contains(x.NodeID))
                                                      .ExecuteDeleteAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
                tx.Commit();
            }
            else
            {
                result = await _lumStoreContext
                    .DocumentPages
                    .Where(x => _lumStoreContext.DocumentNodes
                    .Where(x => nodeIDs.Length == 1 ? (x.NodeID == nodeIDs[0] || x.ParentNodeID == nodeIDs[0])
                        : x.ParentNodeID != null && (nodeIDs.Contains(x.NodeID) || nodeIDs.Contains(x.ParentNodeID.Value))).Select(s => s.NodeID).Contains(x.NodeID))
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, true));
            }

            if (result > 0)
            {
                var cacheDependencies = new CacheDependency().Nodes();
                foreach (var nodeId in nodeIDs)
                {
                    cacheDependencies.NodeID(nodeId);
                }
                _cacheService.TouchKey(cacheDependencies.GetDependencies().ToArray());
            }
            return result;
        }

        public async Task<string> GetRelativeUrl(int nodeID)
        {
            var data = await _lumStoreContext.DocumentLinkedNodes
                 .Join(_lumStoreContext.DocumentNodes,
                 ln => ln.Ancestor,
                 n => n.NodeID,
                 (ln, n) => new { alias = n.NodeAlias, nodeOrder = n.NodeOrder, nodeID = ln.Descendant })
                 .Where(x => x.nodeID == nodeID)
                 .ToListAsync();

            return string.Join("/", data.OrderBy(x => x.nodeOrder).Select(x => x.alias));
        }

        public async Task<T?> InsertAsync<T>(T page, DocumentNode? parent = null) where T : DocumentPage
        {
            using var tx = await _lumStoreContext.Database.BeginTransactionAsync();


            var documentPage = page.GetType().GetProperties()
                .FirstOrDefault(p => Attribute.IsDefined(p, typeof(DocumentNameAttribute)))?.GetValue(page)?.ToString();

            page.DocumentName = ValidationHelper.GetStringValue(documentPage, page.DocumentName);
            var alias = page.DocumentName.Slug;

            var parentNodeId = parent?.NodeID;
            var isHasAlias = await TreeNodeHelper.ExistsAliasAsync(_lumStoreContext, alias, parentNodeId);

            if (isHasAlias)
            {
                alias += "-" + Guid.NewGuid();
            }

            var maxOrder = await TreeNodeHelper.GetMaxOrderAsync(_lumStoreContext, parentNodeId);
            var nodeAliasPath = await TreeNodeHelper.GetNodeAliasPath(_lumStoreContext, alias, parent?.NodeID);

            page.Node.NodeAlias = alias;
            page.Node.NodeOrder = maxOrder + 1;
            page.Node.ParentNodeID = parentNodeId;
            page.Node.NodeName = page.DocumentName;
            page.Node.RelativeUrl = nodeAliasPath;

            try
            {
                await _lumStoreContext.AddAsync(page);
                await _lumStoreContext.SaveChangesAsync();

                await TreeNodeHelper.InsertClosureTable(_lumStoreContext, [page.NodeID], parentNodeId);
                var cacheDependencies = new CacheDependency().Nodes().ClassName(page.Node.ClassName).NodeID(page.NodeID);
                if (parentNodeId.HasValue)
                {
                    cacheDependencies.NodeID(parentNodeId.Value);
                }
                _cacheService.TouchKey(cacheDependencies.GetDependencies().ToArray());
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            await tx.CommitAsync();

            return page;
        }

        public async Task<IEnumerable<T>> InsertsAsync<T>(T[] pages, DocumentNode? parent = null) where T : DocumentPage
        {
            if (pages.Length == 0)
            {
                return Enumerable.Empty<T>();
            }

            using var tx = await _lumStoreContext.Database.BeginTransactionAsync();
            var aliases = pages.Select(x => x.DocumentName.Slug).ToArray();

            var parentNodeId = parent?.NodeID;
            var databaseAliases = await _lumStoreContext.DocumentNodes
                .AsNoTracking()
                .Where(x => aliases.Contains(x.NodeAlias) && x.ParentNodeID == parentNodeId)
                .Select(x => new { oldAlias = x.NodeAlias, newAlias = $"{x.NodeAlias}-{Guid.NewGuid()}" })
                .ToDictionaryAsync(x => x.oldAlias, x => x.newAlias);

            if (databaseAliases.Count > 0)
            {
                for (int i = 0; i < aliases.Length; i++)
                {
                    if (!databaseAliases.ContainsKey(aliases[i]))
                    {
                        databaseAliases.Add(aliases[i], aliases[i]);
                    }
                }
            }

            var maxOrder = await TreeNodeHelper.GetMaxOrderAsync(_lumStoreContext, parentNodeId);

            var cacheDependencies = new CacheDependency().Nodes();
            var parentNodeAliasPath = await TreeNodeHelper.GetNodeAliasPath(_lumStoreContext, "", parent?.NodeID);
            for (int i = 0; i < pages.Length; i++)
            {
                maxOrder++;
                pages[i].Node.NodeAlias = databaseAliases[pages[i].DocumentName.Slug];
                pages[i].Node.NodeOrder = maxOrder + 1;
                pages[i].Node.ParentNodeID = parentNodeId;
                pages[i].Node.RelativeUrl = $"{parentNodeAliasPath}{pages[i].Node.NodeAlias}";
                if (parentNodeId.HasValue)
                {
                    cacheDependencies.NodeID(parentNodeId.Value);
                }
            }

            try
            {
                await _lumStoreContext.AddRangeAsync(pages);
                await _lumStoreContext.SaveChangesAsync();

                await TreeNodeHelper.InsertClosureTable(_lumStoreContext, pages.Select(x => x.NodeID).ToArray(), parentNodeId);
                _cacheService.TouchKey(cacheDependencies.GetDependencies().ToArray());

            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
            await tx.CommitAsync();

            return pages;
        }

        public async Task<bool> MoveAsync(int nodeID, int? parentId = null, int? nestedNodeID = null)
        {
            var currentNode = await _lumStoreContext.DocumentNodes.FindAsync(nodeID);
            if (currentNode == null)
                return false;

            if (parentId != null && !_lumStoreContext.DocumentNodes.Any(x => x.NodeID == parentId))
            {
                throw new InvalidDataException("Cannot find parent node");
            }
            using var tx = await _lumStoreContext.Database.BeginTransactionAsync();
            try
            {
                //Order when case is similar parent
                if (currentNode.ParentNodeID == parentId)
                {
                    await TreeNodeHelper.OrderSameParent(_lumStoreContext, currentNode, nestedNodeID, parentId);
                }
                else
                {
                    await TreeNodeHelper.OrderDifferentParent(_lumStoreContext, currentNode, nestedNodeID, parentId);
                }
                await _lumStoreContext.SaveChangesAsync();
                await tx.CommitAsync();

                var cacheDependencies = new CacheDependency().Nodes().ClassName(currentNode.ClassName).NodeID(nodeID).NodeOrder();
                if (currentNode.ParentNodeID.HasValue)
                {
                    cacheDependencies.NodeID(currentNode.ParentNodeID.Value);
                }
                _cacheService.TouchKey(cacheDependencies.GetDependencies().ToArray());
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public Task<int> RenameNodeAsync(int nodeID, string name)
        {
            _cacheService.TouchKey(new CacheDependency().Nodes().NodeID(nodeID).GetDependencies().ToArray());
            return _lumStoreContext.DocumentNodes.Where(x => x.NodeID == nodeID)
                  .ExecuteUpdateAsync(x => x.SetProperty(p => p.NodeName, name));

        }

        public async Task<bool> UpdateAsync<T>(T page) where T : DocumentPage
        {
            _lumStoreContext.Update(page);
            var isSuccess = await _lumStoreContext.SaveChangesAsync() > 0;
            if (isSuccess)
            {
                string className = DocumentPageTypeHelper.GetClassName<T>();

                _cacheService.TouchKey(new CacheDependency().Nodes().NodeID(page.NodeID).ClassName(className).GetDependencies().ToArray());
            }

            return isSuccess;
        }

        public async Task<bool> UpdateAsync<T>(int nodeID, Action<UpdateSettersBuilder<T>> properties) where T : DocumentPage
        {
            var isSuccess = (await _lumStoreContext.Set<T>()
                .Where(x => x.NodeID == nodeID)
                .ExecuteUpdateAsync(properties)) > 0;

            if (isSuccess)
            {
                var type = typeof(T);

                string className = type.GetField("CLASS_NAME", BindingFlags.Public | BindingFlags.Static)?.GetValue(null)?.ToString() ?? "CMS.Folder";
                _cacheService.TouchKey(new CacheDependency().Nodes().NodeID(nodeID).ClassName(className).GetDependencies().ToArray());
            }

            return isSuccess;
        }

        public async Task<DocumentPage> UpdateAsync(string className, int nodeID, Dictionary<string, object?> properties)
        {
            var type = DocumentPageTypeHelper.DocumentPageTypes.GetValueOrDefault(className, typeof(DocumentPage));
            var page = await _lumStoreContext
                 .DocumentPages
                 .FirstOrDefaultAsync(x => x.NodeID == nodeID);

            if (page == null || page.GetType() != type)
                throw new NullReferenceException($"Cannot found NodeID {nodeID} with class name: {className}");

            foreach (var field in properties)
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
                    prop.SetValue(page, value == null ? null : Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType));
                }
            }

            await _lumStoreContext.SaveChangesAsync();
            _cacheService.TouchKey(new CacheDependency().Nodes().NodeID(nodeID).ClassName(className).GetDependencies().ToArray());
            return page;
        }

        public async Task<int> UpdatesAsync<T>(Action<UpdateSettersBuilder<T>> properties) where T : DocumentPage
        {

            var count = await _lumStoreContext.Set<T>().ExecuteUpdateAsync(properties);
            if (count > 0)
            {
                string className = DocumentPageTypeHelper.GetClassName<T>();
                _cacheService.TouchKey(new CacheDependency().Nodes().ClassName(className).GetDependencies().ToArray());
            }
            return count;
        }

        public async Task<WidgetData<object>[]?> UpdateWidgets(int nodeID, WidgetData<object>[] widgetData)
        {
            int count = await _lumStoreContext.DocumentPages
                  .Where(x => x.NodeID == x.NodeID)
                  .ExecuteUpdateAsync(x => x.SetProperty(p => p.DocumentPageWidgets, widgetData));
            if (count > 0)
            {
                _cacheService.TouchKey(new CacheDependency().NodeID(nodeID).GetDependencies().ToArray());
                return widgetData;

            }
            return null;
        }
    }
}
