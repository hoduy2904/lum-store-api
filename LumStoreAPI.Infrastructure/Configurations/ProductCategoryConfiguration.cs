using LumStoreAPI.Core.Entities.Pages;
using LumStoreAPI.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.Property(x => x.CategoryName)
                .HasMaxLength(100);

            builder.Property(x => x.CategoryImage)
                .HasMaxLength(250)
                .HasConversion(ConverterHelper.ArrayGuidConverter(','))
                .Metadata.SetValueComparer(ValueCompareHelper.GUIDArrayCompare);

            builder.Property(x => x.CategoryDescription)
                .HasMaxLength(200);
        }
    }
}
