using System;
using LumStoreAPI.Application.DTOs.DocumentContents;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Widgets;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using MediatR;

namespace LumStoreAPI.Widgets;

public class CTAImagesWidgetHandler
(
    IPageRetrieveContext pageRetrieveContext,
    IMediaService mediaService
)
 : IRequestHandler<CTAImagesWidget, CTAImagesWidgetDTO>
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    private readonly IMediaService _mediaService = mediaService;
    public async Task<CTAImagesWidgetDTO> Handle(CTAImagesWidget request, CancellationToken cancellationToken)
    {
        var model = new CTAImagesWidgetDTO();
        if (request.PathId == 0) return model;

        var ctaImages = await _pageRetrieveContext.GetPagesAsync<CTAImageItem>(query =>
        {
            query.GetDescendants(request.PathId)
            .IncludeQueryable(x => x.Take(2));
        }, cache => cache.Dependencies(action => action.Children(request.PathId).NodeOrder()).Key($"ctaimages|{request.PathId}"));

        var imageIds = ctaImages.SelectMany(x => x.Image).ToArray();

        var images = await _mediaService.GetMediaItemsAsync(imageIds);
        model.CTAImages = ctaImages.Select(x =>
        {
            var dto = new CTAImageItemDTO(x)
            {
                Image = images.FirstOrDefault(i => x.Image.Contains(i.FileID))?.FileURL
            };
            return dto;
        });

        return model;
    }
}
