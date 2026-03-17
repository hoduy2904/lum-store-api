using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Application.DTOs.EventLogDTO
{
    public class EventLogGet
    {
        public EventLogType EventLogType { get; set; } = EventLogType.INFORMATION;
        public string EventSource { get; set; } = default!;
        public string EventCode { get; set; } = default!;
        public string? EventName { get; set; }
        public string EventDescription { get; set; } = string.Empty;
        public string ServerName { get; set; } = default!;
        public string? IPAddress { get; set; }
        public string? EventUrl { get; set; }
        public string? CreatedBy { get; set; }

        public EventLogGet(EventLog eventLog)
        {
            this.EventLogType = eventLog.EventLogType;
            this.EventSource = eventLog.EventSource;
            this.EventCode = eventLog.EventCode;
            this.EventName = eventLog.EventName;
            this.EventDescription = eventLog.EventDescription;
            this.ServerName = eventLog.ServerName;
            this.IPAddress = eventLog.IPAddress;
            this.EventUrl = eventLog.EventUrl;
            this.CreatedBy = eventLog.User?.FullName;
        }
    }
}
