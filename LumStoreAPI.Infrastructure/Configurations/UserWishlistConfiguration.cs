using LumStoreAPI.Core.Entities.Customers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class UserWishlistConfiguration : IEntityTypeConfiguration<UserWishlist>
{
    public void Configure(EntityTypeBuilder<UserWishlist> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.UserId, x.NodeId }).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.Node)
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
