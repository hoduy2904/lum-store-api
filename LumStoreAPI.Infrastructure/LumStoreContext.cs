using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.Customers;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Entities.Integrations;
using LumStoreAPI.Core.Entities.Orders;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using LumStoreAPI.Infrastructure.Interceptors;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace LumStoreAPI.Infrastructure
{
    public partial class LumStoreContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        protected readonly IConfiguration Configuration;
        protected readonly ICacheService _cacheService;
        public LumStoreContext(DbContextOptions<LumStoreContext> options, IConfiguration configuration, ICacheService cacheService, IServiceProvider serviceProvider)
            : base(options)
        {
            Configuration = configuration;
            _cacheService = cacheService;
            _serviceProvider = serviceProvider;
        }
        public DbSet<User> Users { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<DocumentNode> DocumentNodes { get; set; }
        public DbSet<DocumentPage> DocumentPages { get; set; }
        public DbSet<DocumentLinkedNode> DocumentLinkedNodes { get; set; }
        public DbSet<HomePage> HomePages { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<SettingKeyValue> SettingKeyValues { get; set; }
        public DbSet<EmailQueue> EmailQueues { get; set; }
        public DbSet<MediaLibraryCategory> MediaLibraryCategories { get; set; }
        public DbSet<MediaLibrary> MediaLibraries { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ContactUs> ContactUs { get; set; }

        // ── Orders ────────────────────────────────────────────────────────────
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<OrderNote> OrderNotes { get; set; }
        public DbSet<OrderReturn> OrderReturns { get; set; }
        public DbSet<OrderReturnItem> OrderReturnItems { get; set; }

        // ── Customers ─────────────────────────────────────────────────────────
        public DbSet<CustomerProfile> CustomerProfiles { get; set; }
        public DbSet<LoyaltyPoint> LoyaltyPoints { get; set; }
        public DbSet<CustomerTier> CustomerTiers { get; set; }
        public DbSet<CustomerNote> CustomerNotes { get; set; }
        public DbSet<DiscountRule> DiscountRules { get; set; }

        // ── Integrations ──────────────────────────────────────────────────────
        public DbSet<IntegrationConfig> IntegrationConfigs { get; set; }
        public DbSet<SyncLog> SyncLogs { get; set; }

        // ── Wishlist ───────────────────────────────────────────────────────────
        public DbSet<UserWishlist> UserWishlists { get; set; }

        // ── Cart ───────────────────────────────────────────────────────────────
        public DbSet<UserCart> UserCarts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = Configuration.GetConnectionString("LumDbContext");
                optionsBuilder.UseSqlServer(connectionString);
            }
            optionsBuilder.AddInterceptors(new ProductInterceptor(_serviceProvider));
            base.OnConfiguring(optionsBuilder);
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries();
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    if (entry.Entity is BaseItem entity)
                    {
                        entity.CreatedAt = now;
                        entity.UpdatedAt = now;
                    }
                    if (entry.Entity is DocumentPage documentPage)
                    {
                        string className = DocumentPageTypeHelper.GetClassName(entry.Entity.GetType());

                        foreach (var cache in new CacheDependency().ClassName(className).NodeOrder().Nodes().GetDependencies())
                        {
                            _cacheService.TouchKey(cache);
                        }
                    }

                }
                else if (entry.State == EntityState.Modified)
                {
                    if (entry.Entity is BaseItem entity)
                    {
                        entity.UpdatedAt = now;
                    }
                    if (entry.Entity is DocumentPage documentPage)
                    {
                        string className = DocumentPageTypeHelper.GetClassName(entry.Entity.GetType());
                        foreach (var cache in new CacheDependency().NodeID(documentPage.NodeID).ClassName(className).NodeOrder().GetDependencies())
                        {
                            _cacheService.TouchKey(cache);
                        }
                    }
                    if (entry.Entity is DocumentNode documentNode)
                    {
                        foreach (var cache in new CacheDependency().NodeID(documentNode.NodeID).NodeOrder().Nodes().GetDependencies())
                        {
                            _cacheService.TouchKey(cache);
                        }
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    if (entry.Entity is DocumentPage documentPage)
                    {
                        string className = DocumentPageTypeHelper.GetClassName(entry.Entity.GetType());
                        foreach (var cache in new CacheDependency().NodeID(documentPage.NodeID).ClassName(className).NodeOrder().GetDependencies())
                        {
                            _cacheService.TouchKey(cache);
                        }
                    }
                    if (entry.Entity is DocumentNode documentNode)
                    {
                        foreach (var cache in new CacheDependency().NodeID(documentNode.NodeID).NodeOrder().Nodes().GetDependencies())
                        {
                            _cacheService.TouchKey(cache);
                        }
                    }
                }
            }
        }
    }
}