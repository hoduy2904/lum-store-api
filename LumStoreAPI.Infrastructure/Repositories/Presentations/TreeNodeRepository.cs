using LumStoreAPI.Core.Attributes;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Infrastructure;
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
        private readonly LumStoreContext _lumStoreContext;
        public TreeNodeRepository(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        public Task<int> DeleteAsync(int nodeID, bool hardDelete = false)
        {
            return DeletesAsync([nodeID], hardDelete);
        }

        public async Task<int> DeletesAsync(int[] nodeIDs, bool hardDelete = false)
        {
            if (nodeIDs.Length == 0)
                return 0;
            if (hardDelete)
            {
                using var tx = await _lumStoreContext.Database.BeginTransactionAsync();
                int result = 0;
                try
                {
                    await _lumStoreContext.DocumentLinkedNodes
                       .Where(x => _lumStoreContext.DocumentLinkedNodes
                                                   .Where(x => nodeIDs.Length == 1 ? x.Ancestor == nodeIDs[0] : nodeIDs.Contains(x.Ancestor))
                                                   .Select(s => s.Descendant)
                                                   .Contains(x.Descendant))
                       .ExecuteDeleteAsync();

                    await _lumStoreContext.DocumentNodes
                                                    .Where(x => x.ParentNodeID != null && (nodeIDs.Length == 1 ? x.ParentNodeID == nodeIDs[0] : nodeIDs.Contains(x.ParentNodeID.Value)))
                                                    .ExecuteDeleteAsync();

                    result = await _lumStoreContext.DocumentNodes
                                .Where(x => nodeIDs.Length == 0 ? x.NodeID == nodeIDs[0] : nodeIDs.Contains(x.NodeID))
                                .ExecuteDeleteAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
                tx.Commit();
                return result;
            }
            else
            {
                return await _lumStoreContext
                    .DocumentPages
                    .Where(x => _lumStoreContext.DocumentNodes
                    .Where(x => nodeIDs.Length == 1 ? (x.NodeID == nodeIDs[0] || x.ParentNodeID == nodeIDs[0])
                        : x.ParentNodeID != null && (nodeIDs.Contains(x.NodeID) || nodeIDs.Contains(x.ParentNodeID.Value))).Select(s => s.NodeID).Contains(x.NodeID))
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, true));
            }
        }

        public async Task<string> GetRelativeUrl(int nodeID)
        {
            var data = await _lumStoreContext.DocumentLinkedNodes
                 .Join(_lumStoreContext.DocumentNodes,
                 ln => ln.Ancestor,
                 n => n.NodeID,
                 (ln, n) => new { alias = n.NodeAlias, nodeOrder = n.NodeOrder, nodeID = ln.Descendant })
                 .Where(x=>x.nodeID == nodeID)
                 .ToListAsync();

            return string.Join("/", data.OrderBy(x => x.nodeOrder).Select(x => x.alias));
        }

        public async Task<T?> InsertAsync<T>(T page, DocumentNode? parent = null) where T : DocumentPage
        {
            using var tx = await _lumStoreContext.Database.BeginTransactionAsync();


            var documentPage = page.GetType().GetProperties()
                .FirstOrDefault(p => Attribute.IsDefined(p, typeof(DocumentNameAttribute)))?.GetValue(page)?.ToString();

            page.DocumentName = ValidationHelper.GetStringValue(documentPage, "Folder");
            var alias = page.DocumentName.Slug;

            var parentNodeId = parent?.NodeID;
            var isHasAlias = await TreeNodeHelper.ExistsAliasAsync(_lumStoreContext, alias, parentNodeId);

            if (isHasAlias)
            {
                alias += "-" + Guid.NewGuid();
            }

            var maxOrder = await TreeNodeHelper.GetMaxOrderAsync(_lumStoreContext, parentNodeId);

            var newNode = new DocumentNode
            {
                NodeAlias = alias,
                NodeOrder = maxOrder + 1,
                ParentNodeID = parentNodeId
            };

            page.Node = newNode;

            try
            {
                await _lumStoreContext.AddAsync(page);
                await _lumStoreContext.SaveChangesAsync();

                await TreeNodeHelper.InsertClosureTable(_lumStoreContext, [page.NodeID], parentNodeId);
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

            for (int i = 0; i < pages.Length; i++)
            {
                var newNode = new DocumentNode
                {
                    NodeAlias = databaseAliases[pages[i].DocumentName.Slug],
                    NodeOrder = maxOrder + 1,
                    ParentNodeID = parentNodeId
                };
                maxOrder++;
                pages[i].Node = newNode;
            }

            try
            {
                await _lumStoreContext.AddRangeAsync(pages);
                await _lumStoreContext.SaveChangesAsync();

                await TreeNodeHelper.InsertClosureTable(_lumStoreContext, pages.Select(x => x.NodeID).ToArray(), parentNodeId);
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
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync<T>(T page) where T : DocumentPage
        {
            _lumStoreContext.Update(page);
            return await _lumStoreContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateAsync<T>(int nodeID, Action<UpdateSettersBuilder<T>> properties) where T : DocumentPage
        {
            return (await _lumStoreContext.Set<T>()
                .Where(x => x.NodeID == nodeID)
                .ExecuteUpdateAsync(properties)) > 0;
        }

        public async Task<DocumentPage> UpdateAsync(string className, int nodeID, Dictionary<string, object?> properties)
        {
            var type = DocumentPageTypeHelper.DocumentPageTypes.GetValueOrDefault(className, typeof(DocumentPage));
            var page = await _lumStoreContext
                 .DocumentPages
                 .FirstOrDefaultAsync(x => x.ClassName.Equals(className) && x.NodeID == nodeID);

            if (page == null || page.GetType() != type)
                throw new NullReferenceException($"Cannot found NodeID {nodeID} with class name: {className}");

            foreach (var field in properties)
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
                    prop.SetValue(page, Convert.ChangeType(value, prop.PropertyType));
                }
            }

            await _lumStoreContext.SaveChangesAsync();
            return page;
        }

        public Task<int> UpdatesAsync<T>(Action<UpdateSettersBuilder<T>> properties) where T : DocumentPage
        {
            return _lumStoreContext.Set<T>().ExecuteUpdateAsync(properties);
        }
    }
}
