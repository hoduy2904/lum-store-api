using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentTypeController : ControllerBase
    {
        private readonly IDocumentTableService _documentTableService;
        public DocumentTypeController(IDocumentTableService documentTableService)
        {
            _documentTableService = documentTableService;
        }

        [HttpGet("{className}")]
        public IActionResult GetSchemaTable(string className)
        {
            var documentTable = _documentTableService.GetSchemaTable(className);
            if (documentTable == null)
            {
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND, ["Cannot found this schema table"]));
            }
            return Ok(APIResponse<DocumentTable>.Success(documentTable, ["Success"]));
        }


        [Authorize]
        [HttpGet]
        public IActionResult GetSchemaTables()
        {
            var documentTables = _documentTableService.GetSchemaTables();
            return Ok(APIResponse<IEnumerable<DocumentTable>>.Success(documentTables, ["Successes"]));
        }
    }
}
