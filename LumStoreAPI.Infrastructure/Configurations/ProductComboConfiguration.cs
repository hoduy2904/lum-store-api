using LumStoreAPI.Core.Entities.DocumentTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class ProductComboConfiguration : IEntityTypeConfiguration<ProductCombo>
    {
        public void Configure(EntityTypeBuilder<ProductCombo> builder)
        {
            builder.HasKey(x => new { x.ProductID, x.VariantID });

            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductCombos)
                .HasForeignKey(x => x.ProductID)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.ProductVariant)
               .WithMany(x => x.ProductCombos)
               .HasForeignKey(x => x.VariantID);
        }
    }
}
