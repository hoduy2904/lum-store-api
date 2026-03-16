using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreeNodeManagerController : ControllerBase
    {
        private readonly IPageRetrieveContext _pageRetrieveContext;
        private readonly ITreeNodeRepository _treeNodeRepository;
        public TreeNodeManagerController(IPageRetrieveContext pageRetrieveContext, ITreeNodeRepository treeNodeRepository)
        {
            _pageRetrieveContext = pageRetrieveContext;
            _treeNodeRepository = treeNodeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNodes([FromQuery] PagingModel model)
        {
            var nodes = (await _pageRetrieveContext.GetPagedPagesAsync<DocumentPage>(query =>
            {
                query
                .Paged(model.Page, model.PageSize)
                .IncludeRelativeUrl()
                .IncludeQueryable(nw => nw.OrderBy(o => o.Node.NodeOrder));
            }, cache => cache.Dependencies(d => d.Nodes()).Key("getallNodes"))).Select(x => new DocumentPageGetDTO(x));
            return Ok(PagedResponse<DocumentPageGetDTO>.Success(nodes, model.Page, model.PageSize, ["Success"]));
        }

        [HttpGet("{nodeId}")]
        public async Task<IActionResult> GetNode(int nodeId)
        {
            var node = (await _pageRetrieveContext.GetPagesAsync<DocumentPage>(query =>
            {
                query.IncludeRelativeUrl()
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

        [HttpPost]
        public async Task<IActionResult> Insert(DocumentPageInsertDTO documentPageDTO)
        {
            var documentPage = new DocumentPage()
            {
                ClassName = documentPageDTO.ClassName,
            };

            var document = await _treeNodeRepository.InsertAsync(documentPageDTO.GetEntity()
                , documentPageDTO.ParentNodeID == null ? null : new DocumentNode { NodeID = documentPageDTO.ParentNodeID.Value });
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
    }
}
