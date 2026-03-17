using LumStoreAPI.Application.DTOs.EventLogDTO;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Interfaces.ContentEngine;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LumStoreAPI.Application.Services
{
    internal class EventLogData : IEventLogData
    {
        private readonly LumStoreContext _context;
        public EventLogData(LumStoreContext context)
        {
            _context = context;
        }
        public async Task<EventLogGet?> GetEventLogAsync(int eventID)
        {
            var eventLog = await _context.EventLogs.FindAsync(eventID);
            return eventLog == null ? null : new EventLogGet(eventLog);
        }

        public async Task<IPagedEnumerable<EventLogGet>> GetEventLogsAsync(EventLogRequest request)
        {
            var eventLogs = _context.EventLogs.Where(x =>
                  (string.IsNullOrEmpty(request.EventSource) || x.EventSource.Contains(request.EventSource))
                  || (string.IsNullOrEmpty(request.EventName) || x.EventName != null && x.EventName.Equals(request.EventName)) ||
                  (string.IsNullOrEmpty(request.EventCode) || x.EventCode != null && x.EventCode.Equals(request.EventCode)) ||
                  (string.IsNullOrEmpty(request.IPAddress) || x.IPAddress != null && x.IPAddress.StartsWith(request.EventCode)) ||
                  (request.EventLogType == null || x.EventLogType == request.EventLogType));

            var totalRecords = await eventLogs.CountAsync();

            var data = await eventLogs.Take(request.PageSize).Skip((request.Page - 1) * request.PageSize).ToArrayAsync();

            return data.Select(x => new EventLogGet(x)).AsPagedEnumerable(totalRecords);
        }
    }
}
