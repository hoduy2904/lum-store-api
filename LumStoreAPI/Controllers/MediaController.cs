using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost]
        public async Task<IActionResult> InsertMedia([FromForm] MediaItemInsertRequest request)
        {
            await _mediaService.InsertMediaItemAsync(request);
            return Ok();
        }

        [HttpGet("getFile")]
        public async Task<IActionResult> GetFile([FromQuery] MediaGetFileRequest request)
        {
            var file = await _mediaService.GetMediaItemAsync(request.FileID);
            if (file == null)
            {
                return NotFound();
            }

            var newFileName = file.FileName;
            if (!string.IsNullOrEmpty(request.Format))
            {
                newFileName = System.IO.Path.GetFileNameWithoutExtension(newFileName) + $".{request.Format.TrimStart('.')}";
            }
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(newFileName, out string? contentType))
            {
                contentType = "application/octet-stream";
            }
            return File(MediaLibraryHelper.GetMediaLibraryStream(file), contentType, newFileName);
        }
    }
}
