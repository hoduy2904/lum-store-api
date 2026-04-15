using LumStoreAPI.Application.DTOs.EventLogDTO;
using LumStoreAPI.Core.Interfaces.ContentEngine;

namespace LumStoreAPI.Application.Interfaces
{
    public interface IEventLogData
    {
        Task<IPagedEnumerable<EventLogGet>> GetEventLogsAsync(EventLogRequest request);
        Task<EventLogGet?> GetEventLogAsync(int eventID);
        Task<int> ClearEventLogsAsync();
    }
}
