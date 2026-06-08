using System;
using System.Net;
using System.Net.Http.Headers;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using LumStoreAPI.SDK.Shiprelay.Models.Requests;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.SDK.Shiprelay;

public class ShiprelayClientHandler : DelegatingHandler
{
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;
    private const string SHIPRELAY_TOKEN_CACHE = "shiprelay_token";
    private const string RETRY_HEADER = "X-Retry-Count";
    public ShiprelayClientHandler(IMemoryCache cache, IServiceProvider serviceProvider)
    {
        _cache = cache;
        _serviceProvider = serviceProvider;
    }
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var shiprelayConfiguration = await scope.ServiceProvider.GetRequiredService<IIntegrationConfigRepository>()
                                    .GetConfigByTypeAsync(Core.Models.Enums.IntegrationType.Shiprelay);

        var token = _cache.Get<string>(SHIPRELAY_TOKEN_CACHE);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (request.RequestUri?.IsAbsoluteUri is false)
        {
            request.RequestUri = new Uri(new Uri(shiprelayConfiguration?.BaseUrl ?? ""), request.RequestUri);
        }
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {

            int retryCount = 0;
            if (request.Headers.TryGetValues(RETRY_HEADER, out var values))
            {
                retryCount = int.Parse(values.First());
            }
            if (retryCount >= 3) return response;
            var authService = scope.ServiceProvider.GetRequiredService<IShiprelayAuthService>();
            var baseUrl = shiprelayConfiguration?.BaseUrl ?? "";

            // Best-effort logout of the stale token before fetching a new one.
            var staleToken = _cache.Get<string>(SHIPRELAY_TOKEN_CACHE);
            if (!string.IsNullOrEmpty(staleToken))
            {
                // Best-effort logout — failure is ignored to allow re-auth to proceed.
                _ = authService.LogoutAsync(staleToken, baseUrl);
                _cache.Remove(SHIPRELAY_TOKEN_CACHE);
            }

            var shiprelayCredentials = new ShiprelayCredentials(baseUrl, shiprelayConfiguration?.ApiKey ?? "", shiprelayConfiguration?.ApiSecret ?? "");
            var tokenResponse = await authService.LoginAsync(shiprelayCredentials);
            if (tokenResponse != null)
            {
                // ShipRelay token TTL unknown; using 23h as safe default. Reduce if API returns 401s before expiry.
                _cache.Set<string>(SHIPRELAY_TOKEN_CACHE, tokenResponse.AccessToken,
                    new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23) });
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
                request.Headers.Remove(RETRY_HEADER);
                request.Headers.Add(RETRY_HEADER, (retryCount + 1).ToString());
                return await base.SendAsync(request, cancellationToken);
            }
            else
            {
                return response;
            }
        }

        return response;
    }
}
