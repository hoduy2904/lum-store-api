using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.SDK.Shiprelay;
using LumStoreAPI.SDK.Shiprelay.Handlers;
using LumStoreAPI.SDK.Shiprelay.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace LumStoreAPI.SDK;

public static class SDKConfiguration
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLumStoreSDK()
        {
            services.AddTransient<ShiprelayClientHandler>();
            services.AddHttpClient(nameof(ShiprelayClientHandler), (sp, client) =>
            {
                var scope = sp.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IIntegrationConfigRepository>().GetConfigByTypeAsync(Core.Models.Enums.IntegrationType.Shiprelay).GetAwaiter().GetResult();
                client.BaseAddress = new Uri(config?.BaseUrl ?? "");
            }).AddHttpMessageHandler<ShiprelayClientHandler>();

            services.AddSingleton<IShiprelayAuthService, ShiprelayAuthService>();
            services.AddSingleton<IShiprelayProductService, ShiprelayProductService>();
            services.AddSingleton<IStripeClient>(s =>
            {
                using var scope = s.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IIntegrationConfigRepository>().GetConfigByTypeAsync(Core.Models.Enums.IntegrationType.Payment).GetAwaiter().GetResult();
                return new StripeClient(config?.ApiSecret ?? "");
            });
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
