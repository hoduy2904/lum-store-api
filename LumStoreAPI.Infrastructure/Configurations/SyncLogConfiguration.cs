using LumStoreAPI.Core.Entities.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class SyncLogConfiguration : IEntityTypeConfiguration<SyncLog>
{
    public void Configure(EntityTypeBuilder<SyncLog> builder)
    {
        builder.HasIndex(x => x.IntegrationType);
        builder.HasIndex(x => x.StartedAt);

        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.ErrorDetail).HasMaxLength(-1); // nvarchar(max)
    }
}
