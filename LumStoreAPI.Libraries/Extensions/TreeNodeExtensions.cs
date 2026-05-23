using System;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Libraries.Extensions;

public static class TreeNodeExtensions
{
    extension(IEnumerable<DocumentPage> documentPages)
    {
        public IEnumerable<DocumentPage> MappingTree()
        {
            if (!documentPages.Any()) return [];
            var lookups = documentPages.ToLookup(x => x.Node.ParentNodeID);

            foreach (var node in documentPages)
            {
                node.Children = lookups[node.NodeID].OrderBy(x => x.Node.NodeOrder).ToList();
            }
            var allIds = documentPages.Select(n => n.NodeID).ToHashSet();

            return documentPages
                .Where(n => !n.Node.ParentNodeID.HasValue || !allIds.Contains(n.Node.ParentNodeID.Value))
                .OrderBy(X => X.Node.NodeOrder)
                .ToList();
        }
    }
}
