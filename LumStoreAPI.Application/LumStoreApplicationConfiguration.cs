using LumStoreAPI.Application.DTOs.Responses;
using LumStoreAPI.Application.Interfaces;
using LumStoreAPI.Application.Middlewares;
using LumStoreAPI.Application.Services;
using LumStoreAPI.Core.Interfaces.Repositories;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Constants.Systems;
using LumStoreAPI.Core.Models.Enums;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace LumStoreAPI.Application
{
    public static class LumStoreApplicationConfiguration
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddLumStoreApplicationConfigurations()
            {
                services.AddScoped<IEventLogService, EventLogService>();
                services.AddScoped<IEventLogData, EventLogData>();
                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IMediaService, MediaService>();
                services.AddScoped<IProductVariantService, ProductVariantService>();
                services.AddScoped<IOrderService, OrderService>();
                services.AddScoped<ICustomerService, CustomerService>();
                services.AddScoped<IShiprelayService, ShiprelayService>();
                services.AddScoped<IDashboardService, DashboardService>();
                services.AddScoped<IIntegrationConfigService, IntegrationConfigService>();
                services.AddScoped<IDiscountRuleService, DiscountRuleService>();
                services.AddScoped<IProductService, ProductService>();
                services.AddScoped<IAddressService, AddressService>();
                services.AddScoped<ISettingKeyValueService, SettingKeyValueService>();
                services.AddScoped<IWishlistService, WishlistService>();
                services.AddScoped<ICartService, CartService>();
                services.AddScoped<IStoreOrderService, StoreOrderService>();
                services.AddScoped<IShiprelayWebhookService, ShiprelayWebhookService>();
                services.AddScoped<IPaymentService, PaymentService>();
                services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizeHandler>();
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
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(AppConfiguration.AdminConfiguration.EncodingKey)
                        };

                        opt.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                                {
                                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                                    return Task.CompletedTask;
                                }
                                if (context.Request.Cookies.ContainsKey(AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME))
                                {
                                    context.Token = context.Request.Cookies[AuthSystemConstants.ACCESS_TOKEN_COOKIE_NAME];
                                }

                                return Task.CompletedTask;
                            },
                            OnTokenValidated = async context =>
                            {
                                var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
                                var userTokenRepository = context.HttpContext.RequestServices.GetRequiredService<IUserTokenRepository>();
                                if (!int.TryParse(context.Principal?.Claims.FirstOrDefault(c => c.Type == "id")?.Value, out int userID))
                                {
                                    context.Fail("Invalid User");
                                }

                                var jwtIdStr = context.Principal?.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                                if (!Guid.TryParse(jwtIdStr, out Guid jwtId) || !await userTokenRepository.IsValidToken(jwtId))
                                {
                                    context.Fail("Token is valid or expired");
                                }

                                var userStatus = await userService.CheckAccountStatusAsync(userID);

                                if (userStatus == null)
                                {
                                    context.Fail("Not found this user");
                                }
                                if (userStatus == AccountStatus.LOCKED)
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.Locked;
                                    await context.Response.WriteAsJsonAsync(APIResponseBase.Failure(ErrorStatusNameConstants.ACCOUNT_LOCKED, ["Your account has been locked, please contact the administrator"]));
                                }
                                if (userStatus == AccountStatus.INACTIVE)
                                {
                                    var claims = new List<Claim>
                                    {
                                        new Claim("Status", nameof(AccountStatus.INACTIVE))
                                    };
                                    var appIdentity = new ClaimsIdentity(claims);
                                    context.Principal?.AddIdentity(appIdentity);
                                }
                            }
                        };
                    });

                services.AddAuthorization(opt =>
                {
                    opt.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireRole(nameof(UserRole.ADMIN), nameof(UserRole.USER))
                    .Build();
                });

                return services;
            }

            public IServiceCollection AddCustomSettings(IConfiguration configuration)
            {
                configuration.GetSection("Configuration").Bind(AppConfiguration.AdminConfiguration);
                return services;
            }
        }

        extension(WebApplicationBuilder builder)
        {
            public void AddLumStoreStaticConfiguration()
            {
                MediaLibraryHelper.RootMediaPath = Path.Combine(builder.Environment.WebRootPath, "Medias");
                DocumentPageTypeHelper.RegisterPageTypes();
                DocumentPageTypeHelper.RegisterWidgets();
                DocumentPageTypeHelper.RegisterFeatureQueries();
            }
        }
    }
}
