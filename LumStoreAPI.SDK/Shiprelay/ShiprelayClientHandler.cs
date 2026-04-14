using System;
using System.Net;
using System.Net.Http.Headers;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
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
        var token = _cache.Get<string>(SHIPRELAY_TOKEN_CACHE);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {

            int retryCount = 0;
            if (request.Headers.TryGetValues(RETRY_HEADER, out var values))
            {
                retryCount = int.Parse(values.First());
            }
            if (retryCount >= 3) return response;

            var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IShiprelayAuthService>();
            var tokenResponse = await authService.LoginAsync();
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
