using LumStoreAPI.Application.DTOs.DocumentContents;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using MediatR;

namespace LumStoreAPI.Widgets;

public class LinkListWidgetHandler
(IPageRetrieveContext pageRetrieveContext,
IMediaService mediaService) : IRequestHandler<LinkListWidget, LinkListWidgetDTO>
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    private readonly IMediaService _mediaService = mediaService;
    public async Task<LinkListWidgetDTO> Handle(LinkListWidget request, CancellationToken cancellationToken)
    {
        var model = new LinkListWidgetDTO();
        if (request.ItemPathId == 0) return model;

        var items = await _pageRetrieveContext.GetPagesAsync<LinkListItem>(query =>
        {
            query.GetDescendants(request.ItemPathId)
            .Select(x => new LinkListItem
            {
                LinkUrl = x.LinkUrl,
                LinkListIcon = x.LinkListIcon,
                LinkListTitle = x.LinkListTitle
            });
        });

        var images = await _mediaService.GetMediaItemsAsync(items.SelectMany(x => x.LinkListIcon).ToArray());
        model.Items = items.Select(x =>
        {
            var image = images.FirstOrDefault(i => x.LinkListIcon.Contains(i.FileID))?.FileURL;
            var item = new LinkListItemDTO(x, image);
            return item;
        });

        return model;
    }
}
