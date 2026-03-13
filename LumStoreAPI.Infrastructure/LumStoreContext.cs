using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Core.Entities.Systems;
using LumStoreAPI.Core.Interfaces.Sytems;
using LumStoreAPI.Core.Models.Riches;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace LumStoreAPI.Infrastructure
{
    public class LumStoreContext : DbContext
    {
        protected readonly IConfiguration Configuration;
        protected readonly ICacheService _cacheService;
        public LumStoreContext(DbContextOptions<LumStoreContext> options, IConfiguration configuration, ICacheService cacheService)
            : base(options)
        {
            Configuration = configuration;
            _cacheService = cacheService;
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
        public DbSet<ProductCategory> ProductCategories { get; set; }

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
                        foreach (var cache in new CacheDependency().ClassName(documentPage.ClassName).NodeOrder().Nodes().GetDependencies())
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
                        foreach (var cache in new CacheDependency().NodeID(documentPage.NodeID).ClassName(documentPage.ClassName).NodeOrder().GetDependencies())
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
                        foreach (var cache in new CacheDependency().NodeID(documentPage.NodeID).ClassName(documentPage.ClassName).NodeOrder().GetDependencies())
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
