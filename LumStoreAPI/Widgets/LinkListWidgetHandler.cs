using LumStoreAPI.Application.DTOs.DocumentContents;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using MediatR;

namespace LumStoreAPI.Widgets;

public class LinkListWidgetHandler(
    IPageRetrieveContext pageContext,
    IMediaService mediaService,
    ILogger<LinkListWidgetHandler> logger
) : IRequestHandler<LinkListWidget, LinkListWidgetDTO>
{
    public async Task<LinkListWidgetDTO> Handle(LinkListWidget request, CancellationToken cancellationToken)
    {
        var model = new LinkListWidgetDTO { ItemPathId = request.ItemPathId };
        if (request.ItemPathId == 0) return model;

        // Step 1: Fetch direct children ordered by NodeOrder, projecting only the three
        // fields consumed downstream. GetChildren uses Node.ParentNodeID (not the closure
        // table) so it works even when DocumentLinkedNode rows are missing.
        // OrderBy is applied before Select so EF Core can include the JOIN for NodeOrder
        // in its ORDER BY clause without needing it in the SELECT list.
        var items = (await pageContext.GetPagesAsync<LinkListItem>(q =>
            q.GetChildren(request.ItemPathId)
             .OrderBy(x => x.Node.NodeOrder)
             .Select(x => new LinkListItem
             {
                 NodeID        = x.NodeID,
                 LinkListTitle = x.LinkListTitle,
                 LinkListIcon  = x.LinkListIcon,
                 LinkUrl       = x.LinkUrl
             })
        )).ToList();

        logger.LogDebug(
            "LinkListWidget itemPathId={ItemPathId}: fetched {Count} item(s)",
            request.ItemPathId, items.Count);

        if (items.Count == 0) return model;

        // Step 2: Collect every icon GUID across all items, deduplicate, then resolve
        // all images in a single IMediaService call — no N+1.
        // Guard skips the network/DB round-trip entirely when no item has an icon.
        var allIconIds = items
            .SelectMany(x => x.LinkListIcon)
            .Distinct()
            .ToArray();

        var images = allIconIds.Length > 0
            ? await mediaService.GetMediaItemsAsync(allIconIds)
            : [];

        // Step 3: Map to DTOs. The list order from Step 1 already reflects NodeOrder,
        // so no secondary sort is needed here.
        model.Items = items.Select(x =>
        {
            var icon = images.FirstOrDefault(i => x.LinkListIcon.Contains(i.FileID))?.FileURL;
            return new LinkListItemDTO(x, icon);
        });

        return model;
    }
}
