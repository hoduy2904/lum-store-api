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
            var nodes = await _pageRetrieveContext.GetPagedPagesAsync<DocumentPage>(query =>
            {
                query
                .Paged(page, pageSize)
                .IncludeRelativeUrl()
                .IncludeQueryable(nw => nw.OrderBy(o => o.Node.NodeOrder));
            });
            return Ok(new
            {
                totalRecords = nodes.TotalRecords,
                data = nodes
            });
        }

        [HttpPost]
        public async Task<IActionResult> ReOrderNode(int nodeID, int? parentNodeID, int? afterNodeID, bool isBefore = true)
        {
            var isOrder = await _treeNodeRepository.MoveAsync(nodeID, parentNodeID, afterNodeID);
            return Ok(isOrder);
        }

        [HttpPost("Insert")]
        public async Task<IActionResult> Insert(string documentName, int? nodeID = null)
        {
            var document = await _treeNodeRepository.InsertAsync(new DocumentPage
            {
                DocumentName = documentName,
                ClassName = "Default",

            }, nodeID == null ? null : new DocumentNode { NodeID = nodeID.Value });

            return Ok(document);
        }
    }
}
