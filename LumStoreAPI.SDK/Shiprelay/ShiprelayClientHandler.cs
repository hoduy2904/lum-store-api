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
            var shiprelayCredentials = new ShiprelayCredentials(shiprelayConfiguration?.BaseUrl ?? "", shiprelayConfiguration?.ApiKey ?? "", shiprelayConfiguration?.ApiSecret ?? "");
            var tokenResponse = await authService.LoginAsync(shiprelayCredentials);
            if (tokenResponse != null)
            {
                _cache.Set<string>(SHIPRELAY_TOKEN_CACHE, tokenResponse.AccessToken);
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
