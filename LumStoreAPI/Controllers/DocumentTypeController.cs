using LumStoreAPI.Infrastructure;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentTypeController : ControllerBase
    {
        private readonly LumStoreContext _lumStoreContext;
        public DocumentTypeController(LumStoreContext lumStoreContext)
        {
            _lumStoreContext = lumStoreContext;
        }
        [HttpGet]
        public IActionResult GetSchemaTableFields(string className)
        {
            return Ok(_lumStoreContext.Model
                 .FindEntityType(DocumentPageTypeHelper.DocumentPageTypes[className])?
                 .GetProperties()
                 .Select(x => new
                 {
                     x.Name,
                     type = x.ClrType.Name,
                     maxLength = x.GetMaxLength(),
                     x.IsNullable,
                 }));
        }
    }
}
