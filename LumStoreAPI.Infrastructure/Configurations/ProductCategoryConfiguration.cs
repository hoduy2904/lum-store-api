using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.Property(x => x.CategoryName)
                .HasMaxLength(200);

            builder.Property(x => x.CategoryImage)
                .HasMaxLength(500);

            builder.Property(x => x.CategoryDescription)
                .HasMaxLength(500);
        }
    }
}
