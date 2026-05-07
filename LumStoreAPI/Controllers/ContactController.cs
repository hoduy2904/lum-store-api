using LumStoreAPI.Application.DTOs.ContactDTO;
using LumStoreAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LumStoreAPI.Controllers;

[Route("api/contact")]
[ApiController]
public class ContactController(IContactService contactService) : ControllerBase
{
    [HttpPost("send-message")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage(SendMessageRequest request)
    {
        var result = await contactService.SendMessageAsync(request);
        return Ok(result);
    }
}
