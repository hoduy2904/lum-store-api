using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasIndex(x => x.SKU).IsUnique();

            builder.HasIndex(x => x.UPC);
            builder.HasIndex(x => x.ProductName);

            builder.Property(x => x.ProductName)
                .HasMaxLength(100);

            builder.Property(x => x.SKU)
                .HasMaxLength(100);

            builder.Property(x => x.ShortDescription)
                .HasMaxLength(100);

            builder.Property(x => x.UPC)
                .HasMaxLength(14);

            builder.Property(x => x.Description)
                .HasMaxLength(-1);
        }
    }
}
