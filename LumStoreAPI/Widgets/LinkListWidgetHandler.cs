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
    ILogger<LinkListWidgetHandler> logger
) : IRequestHandler<LinkListWidget, LinkListWidgetDTO>
{
    public async Task<LinkListWidgetDTO> Handle(LinkListWidget request, CancellationToken cancellationToken)
    {
        var model = new LinkListWidgetDTO { ItemPathId = request.ItemPathId };
        if (request.ItemPathId == 0) return model;

        var items = (await pageContext.GetPagesAsync<LinkListItem>(q =>
            q.GetChildren(request.ItemPathId)
             .OrderBy(x => x.Node.NodeOrder)
             .Select(x => new LinkListItem
             {
                 NodeID = x.NodeID,
                 LinkListTitle = x.LinkListTitle,
                 IconName = x.IconName,
                 LinkUrl = x.LinkUrl
             }), cache => cache.Dependencies(d => d.Children(request.ItemPathId).NodeOrder()).Key("linklistwidget" + request.ItemPathId)
        )).ToList();

        logger.LogDebug(
            "LinkListWidget itemPathId={ItemPathId}: fetched {Count} item(s)",
            request.ItemPathId, items.Count);

        model.Items = items.Select(x => new LinkListItemDTO(x));

        return model;
    }
}
