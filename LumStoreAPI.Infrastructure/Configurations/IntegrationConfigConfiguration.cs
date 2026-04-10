using LumStoreAPI.Core.Entities.Integrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class IntegrationConfigConfiguration : IEntityTypeConfiguration<IntegrationConfig>
{
    public void Configure(EntityTypeBuilder<IntegrationConfig> builder)
    {
        builder.HasIndex(x => x.IntegrationType);

        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BaseUrl).HasMaxLength(500);
        builder.Property(x => x.ApiKey).HasMaxLength(500);
        builder.Property(x => x.ApiSecret).HasMaxLength(500);
        builder.Property(x => x.WebhookSecret).HasMaxLength(500);
        builder.Property(x => x.AdditionalConfig).HasMaxLength(-1); // nvarchar(max) for JSON
    }
}
