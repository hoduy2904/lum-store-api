using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        private const char SPLIT_CHAR = ',';
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            var guidArrayComparer = new ValueComparer<Guid[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            builder.HasKey(x => x.ItemID);

            builder.HasIndex(x => x.SKU).IsUnique();

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductID);

            builder.Property(x => x.SKU)
                .HasMaxLength(30);

            builder.Property(x => x.Color)
                .HasMaxLength(12);

            builder.Property(x => x.UPC)
                .HasMaxLength(32);

            builder.Property(x => x.VariantName).HasMaxLength(30);

            builder.Property(x => x.Images)
                .HasConversion(
                x => string.Join(SPLIT_CHAR, x),
                x => x.Split(SPLIT_CHAR).Select(Guid.Parse).ToArray())
                .Metadata.SetValueComparer(guidArrayComparer);
        }
    }
}
