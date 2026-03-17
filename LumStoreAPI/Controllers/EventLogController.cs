using LumStoreAPI.Application.DTOs.EventLogDTO;
using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.ADMIN))]
    public class EventLogController : ControllerBase
    {
        private readonly IEventLogData _eventLogData;
        public EventLogController(IEventLogData eventLogData)
        {
            _eventLogData = eventLogData;
        }
        [HttpGet]

        public async Task<IActionResult> GetEventLogs([FromQuery] EventLogRequest eventLogRequest)
        {
            var data = await _eventLogData.GetEventLogsAsync(eventLogRequest);
            return Ok(APIResponse<IEnumerable<EventLogGet>>.Success(data));
        }

        [HttpGet("{eventID}")]
        public async Task<IActionResult> GetEventLog(int eventID)
        {
            var data = await _eventLogData.GetEventLogAsync(eventID);
            if (data == null)
                return NotFound(APIResponseBase.Failure(ErrorStatusNameConstants.NOT_FOUND));

            return Ok(APIResponse<EventLogGet>.Success(data));
        }
    }
}
