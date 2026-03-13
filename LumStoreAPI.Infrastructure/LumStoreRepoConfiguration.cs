using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Services;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Identity;
using LumStoreAPI.Infrastructure.Repositories.Interfaces;
using LumStoreAPI.Infrastructure.Repositories.Presentations;
using LumStoreAPI.Infrastructure.Systems;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserTokenRepository, UserTokenRepository>();
                services.AddScoped<IJwtTokenService, JwtTokenService>();
                return services;
            }

            public IServiceCollection AddCMSCache()
            {
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, CacheService>();
                return services;
            }

            public IServiceCollection AddJwtAuthentication()
            {
                services.AddAuthentication(opt =>
                {
                    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                    .AddJwtBearer(opt =>
                    {
                        opt.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = false,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(AppConfiguration.JwtSettings.EncodingKey)
                        };
                    });

                return services;
            }

            public IServiceCollection AddCustomSettings(IConfiguration configuration)
            {
                configuration.GetSection("Configuration").Bind(AppConfiguration.JwtSettings);
                return services;
            }
        }
    }
}
