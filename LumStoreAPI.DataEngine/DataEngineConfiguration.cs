using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Infrastructure.Presentation;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.DataEngine
{
    public static class DataEngineConfiguration
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddDataEngine()
            {
                services.AddScoped<IPageRetrieveContext, PageRetrieveContext>();
                return services;
            }
        }
    }
}
