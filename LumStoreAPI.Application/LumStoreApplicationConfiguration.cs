using LumStoreAPI.Application;
using LumStoreAPI.Application.Services;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Infrastructure;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Builder;
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
                services.AddScoped<IMediaLibraryService, MediaLibraryService>();
                return services;
            }
        }

        extension(WebApplicationBuilder builder)
        {
            public void AddLumStoreStaticConfiguration()
            {
                MediaLibraryHelper.RootMediaPath = Path.Combine(builder.Environment.WebRootPath, "Medias");
                DocumentPageTypeHelper.RegisterPageTypes();
            }
        }
    }
}
