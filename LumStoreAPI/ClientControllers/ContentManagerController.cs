using System.Web;
using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Libraries.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.ClientControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentManagerController(
        IPageRetrieveContext pageRetrieveContext,
        IMediator mediator
        ) : ControllerBase
    {
        private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
        private readonly IMediator _mediator = mediator;

        [HttpGet("{alias}")]
        public async Task<IActionResult> Index(string? alias)
        {
            alias = HttpUtility.UrlDecode(alias);
            var node = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query
                    .Where(x => string.IsNullOrEmpty(alias) ? x.Node.RelativeUrl.Equals(string.Empty) : x.Node.RelativeUrl.Equals(alias))
                        .Published(Core.Models.Enums.TreeNodePublished.Published)
                        .OnlyPages()
                        .IncludeQueryable(q => q.Take(1));
            })).Select(x => new DocumentClientGetDTO(x)).FirstOrDefault();

            if (node is null || !node.IsPublished)
            {
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, [$"Not found page with alias: {alias}"]));
            }

            if (node.FeatureQuery != null)
            {
                node.Fields = await _mediator.Send(node.FeatureQuery);
            }

            foreach (var x in node.DocumentPageWidgets)
            {
                if (x.Properties != null && DocumentPageTypeHelper.DocumentWidgets.TryGetValue(x.WidgetCode, out var widgetType) && x.Properties.GetType() == widgetType)
                {
                    x.Properties = await _mediator.Send(x.Properties);
                }
            }

            return Ok(APIResponse<DocumentClientGetDTO>.Success(node, ["Get success"]));
        }
    }
}
