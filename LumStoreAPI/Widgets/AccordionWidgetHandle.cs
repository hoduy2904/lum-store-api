using LumStoreAPI.Application.DTOs.DocumentContents;
using LumStoreAPI.Application.DTOs.Widgets;
using LumStoreAPI.Application.Widgets;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Interfaces.Repositories;
using MediatR;

namespace LumStoreAPI.Widgets;

public class AccordionWidgetHandle
(
    IPageRetrieveContext pageRetrieveContext
)
 : IRequestHandler<AccordionWidget, AccordionWidgetDTO>
{
    private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
    public async Task<AccordionWidgetDTO> Handle(AccordionWidget request, CancellationToken cancellationToken)
    {
        var model = new AccordionWidgetDTO(request.Title, request.Description);
        if (request.PathId == 0) return model;

        var items = await _pageRetrieveContext.GetPagesAsync<AccordionItem>(query =>
        {
            query.GetDescendants(request.PathId);
        }, cache => cache.Dependencies(d => d.Children(request.PathId).NodeOrder()).Key($"accordion|items|{request.PathId}"));

        model.Items = items.Select(x => new AccordionItemDTO(x)).ToArray();

        return model;
    }
}
