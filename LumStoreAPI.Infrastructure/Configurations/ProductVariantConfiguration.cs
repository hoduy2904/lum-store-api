using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        private const char SPLIT_CHAR = ',';

        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.HasIndex(x => x.ShiprelayId);

            builder.HasIndex(x => x.SKU).IsUnique();

            builder.Property(x => x.SKU)
                .HasMaxLength(30);
            builder.Property(x => x.UPC)
                .HasMaxLength(32);

            builder.Property(x => x.VariantName)
                .HasMaxLength(100);

            builder.Property(x => x.Images)
                .HasConversion(ConverterHelper.ArrayGuidConverter(SPLIT_CHAR))
                .Metadata.SetValueComparer(ValueCompareHelper.GUIDArrayCompare);

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductID)
                .HasPrincipalKey(x => x.NodeID);

            builder.HasOne(x => x.Color)
            .WithMany(x => x.ProductVariants)
            .HasForeignKey(x => x.ColorId);

            builder.HasOne(x => x.CasePack)
            .WithMany(x => x.CasePacks)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
