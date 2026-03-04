using LumStoreAPI.Core.Models.Enums;

namespace LumStoreAPI.Core.Interfaces.Sytems
{
    public interface IEventLogService
    {
        Task LogEvent(EventLogType type, string source, string code, string name, string description = "");
        Task LogException(string source, string code, string name, Exception ex);
        Task LogWarning(string source, string code, string name, string description = "");
        Task LogInformation(string source, string code, string name, string description = "");
    }
}
