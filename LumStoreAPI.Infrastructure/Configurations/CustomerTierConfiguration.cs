using LumStoreAPI.Core.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class CustomerTierConfiguration : IEntityTypeConfiguration<CustomerTier>
{
    public void Configure(EntityTypeBuilder<CustomerTier> builder)
    {
        builder.HasIndex(x => x.TierLevel).IsUnique();

        builder.Property(x => x.TierName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BadgeColor).HasMaxLength(20);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DiscountPercent).HasPrecision(5, 2);
    }
}
