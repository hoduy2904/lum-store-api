using LumStoreAPI.Core.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.TierLevel);

        builder.Property(x => x.Phone).HasMaxLength(30);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.LoyaltyPoints)
            .WithOne(x => x.CustomerProfile)
            .HasForeignKey(x => x.CustomerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CustomerNotes)
            .WithOne(x => x.CustomerProfile)
            .HasForeignKey(x => x.CustomerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
