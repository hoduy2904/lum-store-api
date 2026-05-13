using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

public class ColorItemConfiguration : IEntityTypeConfiguration<ColorItem>
{
    public void Configure(EntityTypeBuilder<ColorItem> builder)
    {
        builder.Property(x => x.ColorName)
            .HasMaxLength(50);

        builder.Property(x => x.ColorValue)
        .HasMaxLength(30);

        builder.HasIndex(x => x.ColorName)
            .IsUnique();

        builder.HasIndex(x => x.ColorValue)
         .IsUnique();

        builder.HasOne(x => x.ColorCategory)
        .WithMany(x => x.Colors)
        .HasForeignKey(x => x.CategoryId);

        builder.HasMany(x => x.ProductVariants)
        .WithOne(x => x.Color)
        .HasForeignKey(x => x.ColorId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
