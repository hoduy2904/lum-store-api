using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Infrastructure.Helpers
{
    internal class TreeNodeHelper
    {
        public async static Task<string> GetNodeAliasPath(LumStoreContext lumStoreContext, string alias, int? parentId = null)
        {
            if (parentId == null) return "/" + alias;
            var nodeids = lumStoreContext.DocumentLinkedNodes
                 .Where(x => x.Descendant == parentId);

            var nodeAlias = await lumStoreContext.DocumentNodes
             .Join(
                 nodeids,
                 node => node.NodeID,
                 ld => ld.Ancestor,
                 ((node, ld) => new { node.NodeAlias, ld.Depth })
             )
             .OrderByDescending(x => x.Depth)
             .Where(x => !string.IsNullOrWhiteSpace(x.NodeAlias))
             .Select(x => x.NodeAlias)
             .ToArrayAsync();

            if (nodeAlias.Length == 0) return "/" + alias;
            return '/' + string.Join('/', nodeAlias ?? []) + "/" + alias;
        }

        public static async Task<int> InsertClosureTable(LumStoreContext lumStoreContext, int[] nodeIds, int? parentNodeID = null)
        {
            if (parentNodeID == null)
            {
                await lumStoreContext.DocumentLinkedNodes.AddRangeAsync(nodeIds.Select(nodeId => new DocumentLinkedNode
                {
                    Ancestor = nodeId,
                    Descendant = nodeId,
                    Depth = 0
                }));

                return await lumStoreContext.SaveChangesAsync();
            }
            else
            {
                var ids = string.Join(',', nodeIds);
                return await lumStoreContext.Database.ExecuteSqlRawAsync($@"
                    INSERT INTO DocumentLinkedNodes ({nameof(DocumentLinkedNode.Ancestor)}, {nameof(DocumentLinkedNode.Descendant)}, {nameof(DocumentLinkedNode.Depth)})

                    SELECT P.{nameof(DocumentLinkedNode.Ancestor)}, N.NodeId, P.{nameof(DocumentLinkedNode.Depth)} + 1
                    FROM DocumentLinkedNodes P
                    CROSS JOIN (SELECT value AS NodeId FROM STRING_SPLIT({{1}}, ',')) N
                    WHERE P.{nameof(DocumentLinkedNode.Descendant)} = {{0}}

                    UNION ALL

                    SELECT N.NodeId, N.NodeId, 0
                    FROM (SELECT value AS NodeId FROM STRING_SPLIT({{1}}, ',')) N
                    ", parentNodeID, ids);
            }
        }

        public static Task<int> UpdateClosureTableMoveNode(LumStoreContext lumStoreContext, int nodeID, int? newParentNodeId)
        {
            return lumStoreContext.Database.ExecuteSqlRawAsync($@"
                INSERT INTO DocumentLinkedNodes ({nameof(DocumentLinkedNode.Ancestor)}, {nameof(DocumentLinkedNode.Descendant)}, {nameof(DocumentLinkedNode.Depth)})
                    SELECT 
                        A.{nameof(DocumentLinkedNode.Ancestor)},
                        D.{nameof(DocumentLinkedNode.Descendant)},
                        A.{nameof(DocumentLinkedNode.Depth)} + D.{nameof(DocumentLinkedNode.Depth)} + 1
                    FROM DocumentLinkedNodes A     
                    CROSS JOIN DocumentLinkedNodes D 
                    WHERE A.{nameof(DocumentLinkedNode.Descendant)} = {{0}}
                      AND D.{nameof(DocumentLinkedNode.Ancestor)} = {{1}};
                ", newParentNodeId.HasValue ? newParentNodeId : DBNull.Value, nodeID);
        }

        public static async Task<int> GetMaxOrderAsync(LumStoreContext lumStoreContext, int? parentNodeID)
        {
            if (parentNodeID == null)
            {
                return await lumStoreContext.DocumentNodes
                    .Where(x => x.ParentNodeID == null).
                    MaxAsync(x => (int?)x.NodeOrder) ?? -1;
            }
            return await lumStoreContext.DocumentNodes
               .Join(lumStoreContext.DocumentLinkedNodes,
               dn => dn.NodeID,
               ln => ln.Descendant,
               (dn, ln) => new { dn.NodeOrder, ln.Ancestor, ln.Depth })
               .Where(x => x.Depth == 1 && x.Ancestor == parentNodeID)
               .MaxAsync(x => (int?)x.NodeOrder) ?? -1;
        }

        public static Task<bool> ExistsAliasAsync(LumStoreContext lumStoreContext, string alias, int? parentNodeID)
        {
            return lumStoreContext.DocumentNodes
                .AsNoTracking()
                .AnyAsync(x => x.NodeAlias.Equals(alias) && x.ParentNodeID == parentNodeID);
        }

        public static async Task OrderSameParent(LumStoreContext lumStoreContext, DocumentNode currentNode, int? nestedNodeID, int? parentId)
        {
            int oldNodeOrder = currentNode.NodeOrder;
            if (nestedNodeID == null)
            {
                currentNode.NodeOrder = 0;
            }
            else
            {
                var nestedNode = await lumStoreContext.DocumentNodes.FindAsync(nestedNodeID);
                if (nestedNode != null)
                {
                    if (nestedNode.ParentNodeID != parentId)
                    {
                        throw new InvalidDataException("Invalid parent id not same data you're put");
                    }
                    currentNode.NodeOrder = nestedNode.NodeOrder;
                }
                else
                {
                    currentNode.NodeOrder = (await TreeNodeHelper.GetMaxOrderAsync(lumStoreContext, parentId)) + 1;
                }
            }
            if (oldNodeOrder < currentNode.NodeOrder)
            {
                await lumStoreContext.DocumentNodes
                     .Where(x => x.NodeOrder >= oldNodeOrder && x.NodeOrder <= currentNode.NodeOrder && x.ParentNodeID == currentNode.ParentNodeID)
                     .ExecuteUpdateAsync(x => x.SetProperty(p => p.NodeOrder, p => p.NodeOrder - 1));
            }
            else
            {
                await lumStoreContext.DocumentNodes
                    .Where(x => x.NodeOrder >= currentNode.NodeOrder && x.NodeOrder <= oldNodeOrder && x.ParentNodeID == currentNode.ParentNodeID)
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.NodeOrder, p => p.NodeOrder + 1));
            }
        }

        public static async Task OrderDifferentParent(LumStoreContext lumStoreContext, DocumentNode currentNode, int? newOrderNodeID, int? parentId)
        {
            if (await lumStoreContext.DocumentLinkedNodes.AnyAsync(x => x.Ancestor == currentNode.NodeID && x.Descendant == parentId))
            {
                throw new InvalidDataException("Cannot move into its own subtree");
            }
            if (parentId == null || newOrderNodeID == null)
            {
                currentNode.NodeOrder = await GetMaxOrderAsync(lumStoreContext, parentId);
            }
            else
            {
                var newOrderNode = await lumStoreContext.DocumentNodes.FindAsync(newOrderNodeID);
                if (newOrderNode == null)
                {
                    throw new InvalidDataException("Cannot find this node order");
                }
                currentNode.NodeOrder = newOrderNode.NodeOrder;
            }
            var nodeAliasPath = await GetNodeAliasPath(lumStoreContext, currentNode.NodeAlias, parentId);
            currentNode.ParentNodeID = parentId;
            currentNode.RelativeUrl = nodeAliasPath;

            await lumStoreContext.DocumentNodes.Where(x => x.NodeOrder >= currentNode.NodeOrder && x.NodeID != currentNode.NodeID && x.ParentNodeID == currentNode.ParentNodeID)
                .ExecuteUpdateAsync(x => x.SetProperty(p => p.NodeOrder, p => p.NodeOrder + 1));

            var subTree = lumStoreContext.DocumentLinkedNodes
                .Where(x => x.Ancestor == currentNode.NodeID)
                .Select(x => x.Descendant);

            var oldAncestors = lumStoreContext.DocumentLinkedNodes
                .Where(x => x.Descendant == currentNode.NodeID && x.Ancestor != currentNode.NodeID)
                .Select(x => x.Ancestor);

            await lumStoreContext.DocumentLinkedNodes
                .Where(x => subTree.Contains(x.Descendant) && oldAncestors.Contains(x.Ancestor) && x.Ancestor != x.Descendant)
                .ExecuteDeleteAsync();

            if (currentNode.ParentNodeID != null)
            {
                await lumStoreContext.DocumentNodes
                .Where(x =>
                    lumStoreContext.DocumentLinkedNodes.Where(ld => ld.Ancestor == parentId)
                    .Select(l => l.Descendant)
                    .Distinct()
                    .Contains(x.NodeID))
                    .ExecuteUpdateAsync(x => x.SetProperty(p => p.RelativeUrl, p => p.RelativeUrl.Replace(p.RelativeUrl, currentNode.RelativeUrl)));

                await UpdateClosureTableMoveNode(lumStoreContext, currentNode.NodeID, currentNode.ParentNodeID);
            }
        }
    }
}
