using System;

namespace LumStoreAPI.SDK.Shiprelay.Models.Requests;

public class AuthRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
