using System;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Core.Entities.DocumentEngine;

namespace LumStoreAPI.Application.Helpers;

public static class TreeNodeExtensions
{
    extension(IEnumerable<DocumentPage> documentPages)
    {
        public IEnumerable<DocumentClientGetLinkedDTO> DocumentClientGetLinkeds()
        {
            if (!documentPages.Any()) return [];
            var nodes = documentPages.Select(x => new DocumentClientGetLinkedDTO(x)).ToList();
            var lookups = nodes.ToLookup(x => x.ParentNodeID);

            foreach (var node in nodes)
            {
                node.Children = lookups[node.NodeID].ToList();
            }
            var allIds = nodes.Select(n => n.NodeID).ToHashSet();

            return nodes
                .Where(n => !n.ParentNodeID.HasValue || !allIds.Contains(n.ParentNodeID.Value))
                .ToList();
        }
    }
}
