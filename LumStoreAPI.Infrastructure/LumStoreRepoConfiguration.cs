using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Infrastructure.Identity;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Repositories.Presentations;
using LumStoreAPI.Infrastructure.Systems;
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
                services.AddScoped<IMediaLibraryCategoryRepository, MediaLibraryCategoryRepository>();
                services.AddScoped<ISettingKeyValueRepository, SettingKeyValueRepository>();
                services.AddScoped<IDocumentTableService, DocumentTableService>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserTokenRepository, UserTokenRepository>();
                services.AddScoped<IJwtTokenService, JwtTokenService>();
                services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
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
