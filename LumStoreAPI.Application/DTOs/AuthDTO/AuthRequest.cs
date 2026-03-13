namespace LumStoreAPI.Application.DTOs.AuthDTO
{
    public class AuthRequest
    {
        public string UserName { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool IsRemember { get; set; }
    }
}
