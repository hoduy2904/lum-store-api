using LumStoreAPI.Core.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasIndex(x => x.OrderCode).IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.CreatedAt);

        builder.Property(x => x.OrderCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(150);
        builder.Property(x => x.CustomerEmail).HasMaxLength(200);
        builder.Property(x => x.CustomerPhone).HasMaxLength(30);
        builder.Property(x => x.ShippingAddress).HasMaxLength(500);
        builder.Property(x => x.ShippingCity).HasMaxLength(100);
        builder.Property(x => x.ShippingState).HasMaxLength(100);
        builder.Property(x => x.ShippingZip).HasMaxLength(20);
        builder.Property(x => x.ShippingCountry).HasMaxLength(10);
        builder.Property(x => x.ShiprelayShipmentId).HasMaxLength(100);
        builder.Property(x => x.TrackingNumber).HasMaxLength(100);
        builder.Property(x => x.TrackingUrl).HasMaxLength(500);
        builder.Property(x => x.ShippingCarrier).HasMaxLength(100);
        builder.Property(x => x.ShippingService).HasMaxLength(100);
        builder.Property(x => x.PaymentMethod).HasMaxLength(50);
        builder.Property(x => x.CustomerNote).HasMaxLength(1000);

        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.ShippingFee).HasPrecision(18, 2);
        builder.Property(x => x.Discount).HasPrecision(18, 2);
        builder.Property(x => x.Tax).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.OrderItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderHistories)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderNotes)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.OrderReturns)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
