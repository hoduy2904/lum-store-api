namespace LumStoreAPI.Application.DTOs.AuthDTO
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = default!;
        public string? RefreshToken { get; set; }
    }
}
