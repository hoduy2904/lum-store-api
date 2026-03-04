using LumStoreAPI.Application;
using LumStoreAPI.Application.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.Application
{
    public static class LumStoreApplicationConfiguration
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddLumStoreApplicationConfigurations()
            {
                services.AddScoped<IEventLogService, EventLogService>();
                return services;
            }
        }
    }
}
