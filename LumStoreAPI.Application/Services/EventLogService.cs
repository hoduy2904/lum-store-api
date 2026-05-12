using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Libraries.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LumStoreAPI.Application.Services
{
    internal class EventLogService : IEventLogService
    {
        private readonly ILogger<EventLogService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LumStoreContext _lumStoreContext;
        public EventLogService(LumStoreContext lumStoreContext, IHttpContextAccessor httpContextAccessor, ILogger<EventLogService> logger)
        {
            _lumStoreContext = lumStoreContext;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }
        public async Task LogEvent(EventLogType type, string source, string code, string name, string description = "")
        {
            try
            {
                string? ipAddress = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress.IPAddressString;
                var eventLog = new EventLog
                {
                    EventLogType = type,
                    EventCode = code,
                    EventDescription = description,
                    EventName = name,
                    EventSource = source,
                    IPAddress = ipAddress,
                    ServerName = Environment.MachineName,
                    EventUrl = _httpContextAccessor?.HttpContext == null ? "" : $"{_httpContextAccessor.HttpContext.Request.Path}{_httpContextAccessor?.HttpContext.Request.QueryString}",
                };

                _lumStoreContext.Add(eventLog);
                await _lumStoreContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EventLog Error");
                LogLevel logLevel = LogLevel.Error;
                if (type == EventLogType.WARNING) logLevel = LogLevel.Warning;
                if (type == EventLogType.INFORMATION) logLevel = LogLevel.Information;

                _logger.Log(logLevel, "Source: {source} \r\nName: {name} \r\n Description: {description}", source, name, description);
            }
        }

        public Task LogException(string source, string code, string name, Exception? ex)
        {
            return LogEvent(EventLogType.ERROR, source, code, name, ex?.ToString() ?? string.Empty);
        }

        public Task LogInformation(string source, string code, string name, string description = "")
        {
            return LogEvent(EventLogType.INFORMATION, source, code, name, description);
        }

        public Task LogWarning(string source, string code, string name, string description = "")
        {
            return LogEvent(EventLogType.WARNING, source, code, name, description);
        }
    }
}
