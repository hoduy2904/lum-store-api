using LumStoreAPI.Application.DTOs.DocumentPageDTO;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Interfaces.Repositories;
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
        public async Task<IActionResult> GetNodes(int page, int pageSize)
        {
            var nodes = (await _pageRetrieveContext.GetPagedPagesAsync<DocumentPage>(query =>
            {
                query
                .Paged(page, pageSize)
                .IncludeRelativeUrl()
                .IncludeQueryable(nw => nw.OrderBy(o => o.Node.NodeOrder));
            }, cache => cache.Dependencies(d => d.Nodes()).Key("getallNodes"))).Select(x => new DocumentPageGetDTO(x));
            return Ok(new
            {
                totalRecords = nodes.TotalRecords,
                data = nodes
            });
        }

        [HttpPatch]
        public async Task<IActionResult> ReOrderNode(int nodeID, int? parentNodeID, int? afterNodeID, bool isBefore = true)
        {
            var isOrder = await _treeNodeRepository.MoveAsync(nodeID, parentNodeID, afterNodeID);
            return Ok(isOrder);
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(DocumentPageInsertDTO documentPageDTO)
        {
            var documentPage = new DocumentPage()
            {
                ClassName = documentPageDTO.ClassName,
            };

            var document = await _treeNodeRepository.InsertAsync(documentPageDTO.GetEntity()
                , documentPageDTO.ParentNodeID == null ? null : new DocumentNode { NodeID = documentPageDTO.ParentNodeID.Value });

            return Ok(document);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update(DocumentPageUpdateDTO documentPageDTO)
        {
            var alias = await _treeNodeRepository.GetRelativeUrl(documentPageDTO.NodeID);
            var page = await _treeNodeRepository
                  .UpdateAsync(documentPageDTO.ClassName, documentPageDTO.NodeID, documentPageDTO.Fields);

            return Ok(new DocumentPageGetDTO(page));
        }
    }
}
