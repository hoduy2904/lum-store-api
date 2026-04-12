using System;
using LumStoreAPI.SDK.Shiprelay;

namespace LumStoreAPI.SDK.Extensions;

internal static class ClientFactoryExtensions
{
    extension(IHttpClientFactory httpClientFactory)
    {
        public HttpClient CreateShiprelayClient()
        {
            return httpClientFactory.CreateClient(nameof(ShiprelayClientHandler));
        }
    }
}
