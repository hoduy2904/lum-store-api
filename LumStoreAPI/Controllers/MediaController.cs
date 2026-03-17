using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.DTOs.Responses;
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
        public MediaController(
            IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost]
        public async Task<IActionResult> InsertMedia([FromForm] MediaItemInsertRequest request)
        {
            var files = await _mediaService.InsertMediaItemAsync(request);
            return Ok(APIResponse<IEnumerable<MediaItemDTO>>.Success(files, ["Success"]));
        }

        public async Task<IActionResult> CreateMediaFolder(MediaFolderRequest request)
        {
            var folder = await _mediaService.CreateMediaFolderAsync(request);

            return Ok(APIResponse<MediaFolderDTO>.Success(folder));
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
            return File(MediaLibraryHelper.GetFileStream(file.FullDirectPath), contentType, newFileName);
        }

        [HttpDelete("File/{fileID}")]
        public async Task<IActionResult> DeleteFile(Guid fileID)
        {
            await _mediaService.DeleteFile(fileID);
            return Ok();
        }
    }
}
