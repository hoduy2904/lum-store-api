using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.DTOs.TreeNodeManageDTO;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class TreeNodeManagerController : ControllerBase
    {
        private readonly IPageRetrieveContext _pageRetrieveContext;
        private readonly ITreeNodeRepository _treeNodeRepository;
        private readonly IMediator _mediator;
        public TreeNodeManagerController(IPageRetrieveContext pageRetrieveContext, ITreeNodeRepository treeNodeRepository, IMediator mediator)
        {
            _pageRetrieveContext = pageRetrieveContext;
            _treeNodeRepository = treeNodeRepository;
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetNodes([FromQuery] TreeNodeManageRequest request)
        {
            var nodes = (await _pageRetrieveContext.GetPagedPagesAsync<DocumentPage>(query =>
            {
                query
                .Paged(request.Page, request.PageSize)
                .IncludeQueryable(nw => nw.OfType<DocumentPage>().OrderBy(o => o.Node.NodeOrder))
                .Published(TreeNodePublished.All);

                if (request.ParentID.HasValue)
                {
                    if (request.IsFullNode)
                    {
                        query.GetDescendants(request.ParentID.Value);
                    }
                    else
                    {
                        query.GetDescendants(request.ParentID.Value, 1);
                    }
                }
                else
                {
                    query.Where(x => x.Node.ParentNodeID == null);
                }

                query.Where(x =>
                    (string.IsNullOrEmpty(request.Search) || x.DocumentName.Contains(request.Search))
                    && (string.IsNullOrEmpty(request.ClassName) || x.Node.ClassName.Equals(request.ClassName)))
                    .Select(x => new DocumentPage
                    {
                        DocumentName = x.DocumentName,
                        CreatedAt = x.CreatedAt,
                        Node = x.Node,
                        NodeID = x.NodeID,
                        PageID = x.PageID,
                        PublishedFrom = x.PublishedFrom,
                        PublishedTo = x.PublishedTo,
                        RequireAuthentication = x.RequireAuthentication,
                    });

            }, cache => cache.Dependencies(d => d.Nodes()).Key($"getallNodes|{request.ParentID}|{request.Search}|{request.Page}|{request.PageSize}|{request.IsFullNode}|{request.ClassName}")))
                .Select(x => new DocumentPageGetDTO(x));
            return Ok(PagedResponse<DocumentPageGetDTO>.Success(nodes, request.Page, request.PageSize, ["Success"]));
        }

        [HttpGet("{nodeId}")]
        public async Task<IActionResult> GetNode(int nodeId)
        {
            var node = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query
                .Where(x => x.NodeID == nodeId);
            })).Select(x => new DocumentPageGetDTO(x)).FirstOrDefault();

            if (node == null)
            {
                return NotFound(APIResponse<DocumentPageGetDTO>.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Cannot found node with id: " + nodeId]));
            }

            return Ok(APIResponse<DocumentPageGetDTO>.Success(node, ["Success"]));
        }

        [HttpPatch("ReOrder")]
        public async Task<IActionResult> ReOrderNode(int nodeID, int? parentNodeID, int? afterNodeID, bool isBefore = true)
        {
            var isOrder = await _treeNodeRepository.MoveAsync(nodeID, parentNodeID, afterNodeID);
            if (isOrder)
                return Ok(APIResponseBase.Success(["Success"]));
            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Please try later"]));
        }

        [HttpPatch("Rename")]
        public async Task<IActionResult> RenameNode(DocumentPageRenameRequest request)
        {
            var result = await _treeNodeRepository.RenameNodeAsync(request.NodeID, request.NodeName);
            if (result > 0)
            {
                return Ok(APIResponseBase.Success(["Success"]));
            }
            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Not found this nodeID or Something error"]));
        }

        [HttpPatch("widget/{nodeId}")]
        public async Task<IActionResult> UpdateWidgets(int nodeId, DocumentPageWidgetRequestDTO request)
        {
            var result = await _treeNodeRepository.UpdateWidgets(nodeId, request.Data);
            if (result != null) return Ok(APIResponse<WidgetData<object>[]>.Success(result));
            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Not found"]));
        }

        [HttpPost]
        public async Task<IActionResult> Insert(DocumentPageInsertDTO documentPageDTO)
        {
            var document = await _treeNodeRepository.InsertAsync(documentPageDTO.GetEntity(), new DocumentNode { NodeID = documentPageDTO.ParentNodeID });
            if (document == null)
            {
                return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Cannot create page, Please try later"]));
            }
            return Ok(APIResponse<DocumentPageGetDTO>.Success(new DocumentPageGetDTO(document), ["Created node"]));
        }

        [HttpPut]
        public async Task<IActionResult> Update(DocumentPageUpdateDTO documentPageDTO)
        {
            var page = await _treeNodeRepository
                  .UpdateAsync(documentPageDTO.ClassName, documentPageDTO.NodeID, documentPageDTO.Fields);

            if (page == null)
            {
                return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.SYSTEM_ERROR, ["Cannot update that page, please try later"]));
            }
            return Ok(APIResponse<DocumentPageGetDTO>.Success(new DocumentPageGetDTO(page), ["Updated page"]));
        }

        [HttpDelete("{nodeID}")]
        public async Task<IActionResult> DeleteNode(int nodeID)
        {
            var result = await _treeNodeRepository.DeleteAsync(nodeID, true);
            return Ok(APIResponse<int>.Success(result));
        }

        [HttpPatch("navigation/{nodeId}")]
        public async Task<IActionResult> UpdateNavigation(int nodeId, [FromBody] DocumentPageNavigationDTO navigation)
        {
            var isSuccess = await _treeNodeRepository.UpdateAsync<DocumentPage>(nodeId, p =>
              p.SetProperty(x => x.IsEnableNavigation, navigation.IsEnable)
              .SetProperty(x => x.OgTitle, navigation.OgTitle)
              .SetProperty(x => x.OgDescription, navigation.OgDescription)
              .SetProperty(x => x.OgImage, navigation.OgImage));

            if (isSuccess) return Ok(APIResponseBase.Success(["Updated navigation"]));
            return Ok(APIResponseBase.Failure("Please try again"));
        }
    }
}
