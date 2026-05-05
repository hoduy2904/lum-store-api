using System.Net.Http.Json;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.SDK.Shiprelay.Handlers;

internal class ShiprelayAuthService
(
    IHttpClientFactory httpClientFactory,
    IServiceProvider serviceProvider

) : IShiprelayAuthService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    public async Task<AuthResponse?> LoginAsync(ShiprelayCredentials shiprelayCredentials)
    {
        using var scope = _serviceProvider.CreateScope();
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri((shiprelayCredentials.BaseAddress.TrimEnd('/') ?? "") + "/");
        var response = await client.PostAsJsonAsync("login", new AuthRequest
        {
            Email = shiprelayCredentials.Username,
            Password = shiprelayCredentials.Password
        });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AuthResponse>();
    }
}
