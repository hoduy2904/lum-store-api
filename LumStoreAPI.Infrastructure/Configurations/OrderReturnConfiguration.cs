using LumStoreAPI.Core.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class OrderReturnConfiguration : IEntityTypeConfiguration<OrderReturn>
{
    public void Configure(EntityTypeBuilder<OrderReturn> builder)
    {
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.AdminNote).HasMaxLength(1000);
        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);
        builder.Property(x => x.StripeRefundId).HasMaxLength(150);

        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderReturns)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReviewedBy)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.ReturnItems)
            .WithOne(x => x.OrderReturn)
            .HasForeignKey(x => x.OrderReturnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class OrderReturnItemConfiguration : IEntityTypeConfiguration<OrderReturnItem>
{
    public void Configure(EntityTypeBuilder<OrderReturnItem> builder)
    {
        builder.Property(x => x.Reason).HasMaxLength(500);

        builder.HasOne(x => x.OrderReturn)
            .WithMany(x => x.ReturnItems)
            .HasForeignKey(x => x.OrderReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.OrderItem)
            .WithMany()
            .HasForeignKey(x => x.OrderItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
