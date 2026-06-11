using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LumStoreAPI.Infrastructure.SeedData
{
    public static class LumStoreSeedData
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LumStoreContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                             .CreateLogger(nameof(LumStoreSeedData));

            try
            {
                if (await context.Users.AnyAsync() && await context.DocumentNodes.AnyAsync())
                {
                    logger.LogInformation("Seed data already exists. Skipping.");
                    return;
                }

                logger.LogInformation("Seeding LUM Nails database...");

                await SeedUsersAsync(context);
                await SeedSettingsAsync(context);
                await SeedContent(context);

                logger.LogInformation("LUM Nails database seeded successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedContent(LumStoreContext context)
        {
            var node = await context.HomePages.AddAsync(new HomePage
            {
                DocumentName = "Home",
                PageTitle = "Home",
                Node = new Core.Entities.DocumentEngine.DocumentNode
                {
                    ClassName = HomePage.CLASS_NAME,
                    NodeAlias = "",
                    RelativeUrl = "",
                    NodeName = "Home",
                    NodeOrder = 1,
                }
            });

            await context.SaveChangesAsync();
            await context.DocumentLinkedNodes.AddAsync(new Core.Entities.DocumentEngine.DocumentLinkedNode
            {
                Ancestor = node.Entity.NodeID,
                Depth = 0,
                Descendant = node.Entity.NodeID,
            });
            await context.SaveChangesAsync();
        }

        // ─── Users ────────────────────────────────────────────────────────────────

        private static async Task SeedUsersAsync(LumStoreContext context)
        {
            var users = new List<User>
            {
                new()
                {
                    UserName     = "admin@lumnails.com",
                    UserPassword = HashHelper.HashPassword("123456@#"),
                    FirstName    = "Admin",
                    LastName     = "LUM",
                    Email        = "admin@lumnails.com",
                    IsAdmin      = true,
                    IsVerified   = true,
                    IsEnabled    = true,
                    IsLocked     = false,
                    UserLevel    = 1,
                    TimeLocked   = DateTimeOffset.UtcNow
                },
                new()
                {
                    UserName     = "sarah@lumnails.com",
                    UserPassword = HashHelper.HashPassword("123456@#"),
                    FirstName    = "Sarah",
                    LastName     = "Nguyen",
                    Email        = "sarah@lumnails.com",
                    IsAdmin      = false,
                    IsVerified   = true,
                    IsEnabled    = true,
                    IsLocked     = false,
                    UserLevel    = 2,
                    TimeLocked   = DateTimeOffset.UtcNow
                },
                new()
                {
                    UserName     = "emily@lumnails.com",
                    UserPassword = HashHelper.HashPassword("123456@#"),
                    FirstName    = "Emily",
                    LastName     = "Tran",
                    Email        = "emily@lumnails.com",
                    IsAdmin      = false,
                    IsVerified   = true,
                    IsEnabled    = true,
                    IsLocked     = false,
                    UserLevel    = 2,
                    TimeLocked   = DateTimeOffset.UtcNow
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // ─── Settings ─────────────────────────────────────────────────────────────

        private static async Task SeedSettingsAsync(LumStoreContext context)
        {
            var settings = new List<SettingKeyValue>
            {
                new() { SettingCode = "SITE_NAME",       SettingName = "Site Name",           SettingValue = "LUM Nails" },
                new() { SettingCode = "SITE_URL",        SettingName = "Site URL",            SettingValue = "http://localhost:3000" },
                new() { SettingCode = "CONTACT_EMAIL",   SettingName = "Contact Email",       SettingValue = "hello@lumnails.com" },
                new() { SettingCode = "CURRENCY",        SettingName = "Default Currency",    SettingValue = "USD" },
                new() { SettingCode = "CURRENCY_SYMBOL", SettingName = "Currency Symbol",     SettingValue = "$" },
                new() { SettingCode = "INSTAGRAM_URL",   SettingName = "Instagram URL",       SettingValue = "https://instagram.com/lumnails" },
                new() { SettingCode = "TIKTOK_URL",      SettingName = "TikTok URL",          SettingValue = "https://tiktok.com/@lumnails" },
                new() { SettingCode = "FREE_SHIPPING",   SettingName = "Free Shipping Over",  SettingValue = "50" },
                new() { SettingCode = "TAX_RATE",        SettingName = "Tax Rate (%)",        SettingValue = "8" },
                new() { SettingCode = "Product_Accordion_Shipping",        SettingName = "Shipping & Returns",        SettingValue = "" },
            };

            await context.SettingKeyValues.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }
    }
}
