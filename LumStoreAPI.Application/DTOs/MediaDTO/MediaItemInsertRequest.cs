using Microsoft.AspNetCore.Http;

namespace LumStoreAPI.Application.DTOs.MediaDTO
{
    public class MediaItemInsertRequest
    {
        public int CategoryID { get; set; }
        public IEnumerable<IFormFile> Files { get; set; } = [];
    }
}
