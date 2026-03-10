using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Repositories.Presentations;
using LumStoreAPI.Infrastructure.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LumStoreAPI.Infrastructure
{
    public static class LumStoreRepoConfiguration
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddLumStoreRepoConfigurations()
            {
                services.AddDbContext<LumStoreContext>();
                services.AddScoped<ITreeNodeRepository, TreeNodeRepository>();
                services.AddScoped<IEmailRepository, EmailRepository>();
                services.AddScoped<IEmailService, EmailService>();
                services.AddScoped<IMediaLibraryRepository, MediaLibraryRepository>();
                services.AddScoped<ISettingKeyValueRepository, SettingKeyValueRepository>();
                services.AddScoped<IDocumentTableService, DocumentTableService>();
                return services;
            }

            public IServiceCollection AddCMSCache()
            {
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, CacheService>();
                return services;
            }
        }
    }
}
