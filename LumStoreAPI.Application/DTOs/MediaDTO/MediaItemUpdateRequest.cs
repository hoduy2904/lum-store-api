using Microsoft.AspNetCore.Http;

namespace LumStoreAPI.Application.DTOs.MediaDTO
{
    public class MediaItemUpdateRequest
    {
        public Guid FileID { get; set; }
        public IFormFile File { get; set; } = default!;
    }
}
