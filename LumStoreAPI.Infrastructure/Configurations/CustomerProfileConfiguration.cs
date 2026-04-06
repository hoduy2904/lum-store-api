using LumStoreAPI.Core.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> builder)
    {
        builder.HasKey(x => x.ItemID);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.TierLevel);

        builder.Property(x => x.Phone).HasMaxLength(30);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.ZipCode).HasMaxLength(20);
        builder.Property(x => x.Country).HasMaxLength(10);
        builder.Property(x => x.TotalSpent).HasPrecision(18, 2);

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
