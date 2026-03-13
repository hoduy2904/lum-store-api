namespace LumStoreAPI.Application.DTOs.AuthDTO
{
    public sealed record class TokenRequest(string accessToken, string refreshToken);
}
