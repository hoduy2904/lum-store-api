using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.DocumentPageDTO
{
    public class DocumentPageUpdateDTO
    {
        [Required]
        public int NodeID { get; set; }
        public string ClassName { get; set; } = default!;

        public Dictionary<string, object?> Fields { get; set; } = [];
    }
}
