using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.ProductDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Libraries.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace LumStoreAPI.ClientControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentManagerController(
        IPageRetrieveContext pageRetrieveContext,
        IMediator mediator,
        IProductService productService
        ) : ControllerBase
    {
        private readonly IPageRetrieveContext _pageRetrieveContext = pageRetrieveContext;
        private readonly IMediator _mediator = mediator;
        private readonly IProductService _productService = productService;

        [HttpGet("{alias}/children")]
        public async Task<IActionResult> GetChildren(string alias)
        {
            alias = HttpUtility.UrlDecode(alias);

            var parent = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query
                    .Where(x => x.Node.NodeAlias.Equals(alias))
                    .IncludeQueryable(q => q.Take(1));
            })).FirstOrDefault();

            if (parent is null)
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, [$"Not found node with alias: {alias}"]));

            var children = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(
                query =>
                {
                    query
                        .GetDescendants(parent.NodeID, 1)
                        .Published(Core.Models.Enums.TreeNodePublished.Published)
                        .IncludeQueryable(q => q.OfType<DocumentPage>().OrderBy(o => o.Node.NodeOrder));
                },
                cache => cache.Dependencies(d => d.Nodes().NodeID(parent.NodeID)).Key($"getChildren|{parent.NodeID}")))
                .ToList();

            var nodeIds = children.Select(c => c.NodeID).ToArray();
            var productCounts = await _productService.GetPublishedProductCountsAsync(nodeIds);

            var dtos = children.Select(c =>
            {
                var dto = new DocumentPageGetDTO(c);
                dto.Fields["productCount"] = productCounts.GetValueOrDefault(c.NodeID, 0);
                return dto;
            });

            return Ok(APIResponse<IEnumerable<DocumentPageGetDTO>>.Success(dtos, ["Success"]));
        }

        [HttpGet("categories/{nodeAlias}/products")]
        public async Task<IActionResult> GetCategoryProducts(string nodeAlias, [FromQuery] CategoryProductsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(APIResponseBase.Failure("pageSize cannot exceed 50"));

            nodeAlias = HttpUtility.UrlDecode(nodeAlias);

            var category = (await _pageRetrieveContext.GetPagesAsync<ProductCategory>(query =>
            {
                query
                    .Where(x => x.Node.NodeAlias.Equals(nodeAlias))
                    .IncludeQueryable(q => q.Take(1));
            })).FirstOrDefault();

            if (category is null)
                return NotFound(APIResponse<object>.Failure("Category not found"));

            var products = await _productService.GetProductsByCategoryAsync(category.NodeID, request);

            return Ok(PagedResponse<DocumentClientGetDTO>.Success(products, request.Page, request.PageSize));
        }

        [HttpGet("{alias}")]
        public async Task<IActionResult> Index(string? alias)
        {
            alias = HttpUtility.UrlDecode(alias);
            var node = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query
                    .Where(x => string.IsNullOrEmpty(alias) ? x.Node.NodeAlias.Equals(string.Empty) : x.Node.NodeAlias.Equals(alias))
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
