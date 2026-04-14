using System;
using LumStoreAPI.SDK.Shiprelay;
using LumStoreAPI.SDK.Shiprelay.Handlers;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.SDK;

public static class SDKConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLumStoreSDK()
        {
            services.AddTransient<ShiprelayClientHandler>();
            services.AddHttpClient(nameof(ShiprelayClientHandler), client =>
            {
                client.BaseAddress = new Uri(ShiprelayConfig.ShiprelayUrl ?? "");
            }).AddHttpMessageHandler<ShiprelayClientHandler>();

            services.AddSingleton<IShiprelayAuthService, ShiprelayAuthService>();
            services.AddSingleton<IShiprelayProductService, ShiprelayProductService>();
            return services;
        }
    }

    extension(IConfiguration configuration)
    {
        public void SDKConfigure()
        {
            ShiprelayConfig.Configure(configuration.GetSection("Shiprelay"));
        }
    }
}
