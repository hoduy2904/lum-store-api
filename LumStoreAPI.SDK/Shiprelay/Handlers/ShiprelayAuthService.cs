using System.Net.Http.Json;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;

namespace LumStoreAPI.SDK.Shiprelay.Handlers;

internal class ShiprelayAuthService
(
    IHttpClientFactory httpClientFactory

) : IShiprelayAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(ShiprelayConfig.ShiprelayUrl ?? "");
        var response = await client.PostAsJsonAsync("login", new AuthRequest
        {
            Email = email,
            Password = password
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }

    public Task<AuthResponse?> LoginAsync()
    {
        return LoginAsync(ShiprelayConfig.Email ?? "", ShiprelayConfig.Password ?? "");
    }
}
