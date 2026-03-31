using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(x => x.ProductName);

            builder.Property(x => x.ProductName)
                .HasMaxLength(100);

            builder.Property(x => x.ShortDescription)
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(-1);

            builder.HasMany(x => x.ProductVariants)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductID);

        }
    }
}
