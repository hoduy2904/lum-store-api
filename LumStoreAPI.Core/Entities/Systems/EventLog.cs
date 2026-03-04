using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Entities.Systems
{
    public class EventLog : BaseClassItem
    {
        public EventLogType EventLogType { get; set; } = EventLogType.INFORMATION;
        public string EventSource { get; set; } = default!;
        public string EventCode { get; set; } = default!;
        public string? EventName { get; set; }
        public string EventDescription { get; set; } = string.Empty;
        public string ServerName { get; set; } = default!;
        public string? IPAddress { get; set; }
        public string? EventUrl { get; set; }
    }
}
