using System;
using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

public class ColorCategoryConfiguration : IEntityTypeConfiguration<ColorCategory>
{
    public void Configure(EntityTypeBuilder<ColorCategory> builder)
    {
        builder.Property(x => x.CategoryName)
            .HasMaxLength(70);

        builder.HasIndex(x => x.CategoryName)
        .IsUnique();

        builder.HasMany(x => x.Colors)
        .WithOne(x => x.ColorCategory)
        .HasForeignKey(x => x.CategoryId);
    }
}
