using LumStoreAPI.Core.Entities;
using LumStoreAPI.Core.Entities.Base;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace LumStoreAPI.Infrastructure
{
    public class LumStoreContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public LumStoreContext(DbContextOptions<LumStoreContext> options, IConfiguration configuration)
            : base(options)
        {
            Configuration = configuration;
        }
        public DbSet<DocumentNode> DocumentNodes { get; set; }
        public DbSet<DocumentPage> DocumentPages { get; set; }
        public DbSet<DocumentLinkedNode> DocumentLinkedNodes { get; set; }
        public DbSet<HomePage> HomePages { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<SettingKeyValue> SettingKeyValues { get; set; }
        public DbSet<EmailQueue> EmailQueues { get; set; }
        public DbSet<MediaLibraryCategory> MediaLibraryCategories { get; set; }
        public DbSet<MediaLibrary> MediaLibraries { get; set; }

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
            var entries = ChangeTracker.Entries<BaseItem>();
            var now = DateTime.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
    }
}
