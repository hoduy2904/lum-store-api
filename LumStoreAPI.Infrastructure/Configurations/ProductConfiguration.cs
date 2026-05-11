using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        private const char SPLIT_CHAR = ',';

        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(x => x.ProductName);

            builder.Property(x => x.ProductName)
                .HasMaxLength(200);

            builder.Property(x => x.ShortDescription)
                .HasMaxLength(300);

            builder.Property(x => x.Description)
                .HasMaxLength(-1);  // nvarchar(max)

            builder.Property(x => x.Price)
                .HasPrecision(18, 2);

            builder.Property(x => x.PriceDiscount)
                .HasPrecision(18, 2);

            // Tags stored as comma-separated string
            var stringArrayComparer = new ValueComparer<string[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray());

            builder.Property(x => x.Tags)
                .HasConversion(
                    x => string.Join(SPLIT_CHAR, x),
                    x => x.Split(SPLIT_CHAR, StringSplitOptions.RemoveEmptyEntries))
                .HasMaxLength(500)
                .Metadata.SetValueComparer(stringArrayComparer);

            builder.HasMany(x => x.ProductVariants)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductID)
                .HasPrincipalKey(x => x.NodeID);
        }
    }
}
