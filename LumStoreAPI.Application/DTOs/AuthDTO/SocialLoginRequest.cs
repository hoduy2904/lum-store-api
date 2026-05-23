namespace LumStoreAPI.Application.DTOs.AuthDTO
{
    public class SocialLoginRequest
    {
        public string Provider { get; set; } = default!;
        public string IdToken { get; set; } = default!;
    }
}
