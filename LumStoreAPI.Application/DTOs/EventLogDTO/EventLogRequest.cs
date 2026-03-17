using LumStoreAPI.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LumStoreAPI.Application.DTOs.EventLogDTO
{
    public class EventLogRequest
    {
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;
        [Range(5, int.MaxValue)]
        public int PageSize { get; set; } = 10;
        public EventLogType? EventLogType { get; set; }
        public string? EventSource { get; set; }
        public string? EventCode { get; set; }
        public string? EventName { get; set; }
        public string? IPAddress { get; set; }
        public string? CreatedBy { get; set; }

    }
}
