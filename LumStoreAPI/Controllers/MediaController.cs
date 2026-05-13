using LumStoreAPI.Application.DTOs.MediaDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Org.BouncyCastle.Ocsp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;
        public MediaController(
            IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpGet("Files")]
        public async Task<IActionResult> GetMediaFiles([FromQuery] MediaItemListingRequest request)
        {
            var mediaFiles = await _mediaService.GetMediaItemsAsync(request);
            return Ok(PagedResponse<MediaItemDTO>.Success(mediaFiles, request.Page, request.PageSize, ["Success"]));
        }

        [HttpGet("Folders")]
        public async Task<IActionResult> GetFolders(int page, int pageSize, string search = "")
        {
            var folders = await _mediaService.GetFoldersAsync(page, pageSize, search);

            return Ok(PagedResponse<MediaFolderDTO>.Success(folders, page, pageSize, ["Success"]));
        }

        [HttpPost("Files")]
        public async Task<IActionResult> InsertMedia([FromForm] MediaItemInsertRequest request)
        {
            var files = await _mediaService.InsertMediaItemAsync(request);
            return Ok(APIResponse<IEnumerable<MediaItemDTO>>.Success(files, ["Success"]));
        }

        [HttpPost("Folders")]
        public async Task<IActionResult> CreateMediaFolder(MediaFolderRequest request)
        {
            var folder = await _mediaService.CreateMediaFolderAsync(request);

            return Ok(APIResponse<MediaFolderDTO>.Success(folder));
        }

        [HttpDelete("Folders/{folderID}")]
        public async Task<IActionResult> DeleteFolder(int folderID)
        {
            var result = await _mediaService.DeleteFolderAsync(folderID);
            if (result > 0)
                return Ok(APIResponseBase.Success(["Deleted"]));
            return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
        }


        [HttpPut("Folders/{folderID}")]
        public async Task<IActionResult> RenameFolder(int folderID, MediaFolderRequest request)
        {
            var result = await _mediaService.RenameMediaFolderAsync(folderID, request);
            if (result > 0)
            {
                return Ok(APIResponseBase.Success(["Success"]));
            }

            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
        }

        [HttpGet("getFile/{fileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFile(Guid fileId, [FromQuery] MediaGetFileRequest request)
        {
            var file = await _mediaService.GetMediaItemAsync(fileId);
            if (file == null)
            {
                return NotFound();
            }

            using var inputStream = MediaLibraryHelper.GetFileStream(file.FullDirectPath);
            var newFileName = file.FileName;
            if (!string.IsNullOrEmpty(request.Format) && request.Format.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            {
                newFileName = Path.GetFileNameWithoutExtension(newFileName) + $".{request.Format.TrimStart('.')}";
                using var outputStream = new MemoryStream();
                using (var image = await Image.LoadAsync(inputStream))
                {
                    var encoder = new WebpEncoder
                    {
                        Quality = 75,
                        FileFormat = WebpFileFormatType.Lossy
                    };

                    // 4. Lưu ảnh đã convert vào outputStream
                    await image.SaveAsWebpAsync(outputStream, encoder);
                }
                outputStream.Position = 0;
                return File(outputStream, "image/webp", newFileName);
            }
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(newFileName, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(inputStream, contentType, newFileName);
        }

        [HttpDelete("Files/{fileID}")]
        public async Task<IActionResult> DeleteFile(Guid fileID)
        {
            int count = await _mediaService.DeleteFileAsync(fileID);
            if (count > 0) return Ok(APIResponseBase.Success(["Deleted"]));
            return Ok(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
        }

        [HttpPut("Files")]
        public async Task<IActionResult> UpdateFile(MediaItemUpdateRequest mediaItemUpdateRequest)
        {
            var mediaItem = await _mediaService.UpdateMediaItemAsync(mediaItemUpdateRequest);
            if (mediaItem == null)
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));
            return Ok(APIResponse<MediaItemDTO>.Success(mediaItem, ["Success"]));
        }
    }
}
