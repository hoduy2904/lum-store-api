using LumStoreAPI.Core.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class LoyaltyPointConfiguration : IEntityTypeConfiguration<LoyaltyPoint>
{
    public void Configure(EntityTypeBuilder<LoyaltyPoint> builder)
    {
        builder.HasIndex(x => x.CustomerProfileId);

        builder.Property(x => x.Description).HasMaxLength(300);

        builder.HasOne(x => x.CustomerProfile)
            .WithMany(x => x.LoyaltyPoints)
            .HasForeignKey(x => x.CustomerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
