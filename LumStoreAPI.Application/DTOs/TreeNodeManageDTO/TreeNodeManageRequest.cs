using LumStoreAPI.Application.DTOs.Responses;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LumStoreAPI.Application.DTOs.TreeNodeManageDTO
{
    public class TreeNodeManageRequest : PagingModel
    {
        [Range(1, int.MaxValue)]
        public int? ParentID { get; set; }
        public string? Search { get; set; }
        public bool IsFullNode { get; set; }
        public string? ClassName { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
