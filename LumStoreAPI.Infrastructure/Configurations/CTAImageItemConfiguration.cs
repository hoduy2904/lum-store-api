using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Core.Models.Controls;
using LumStoreAPI.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class CTAImageItemConfiguration : IEntityTypeConfiguration<CTAImageItem>
    {
        public void Configure(EntityTypeBuilder<CTAImageItem> builder)
        {
            builder.Property(x => x.Title)
                .HasMaxLength(70);

            builder.Property(x => x.Pretitle)
                .HasMaxLength(20);

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.Property(x => x.PrimaryButton)
                .HasConversion(ConverterHelper.ContentConverters<LinkControl?>(null));

            builder.Property(x => x.Image)
                .HasMaxLength(250)
                .HasConversion(ConverterHelper.ArrayGuidConverter(','))
                .Metadata.SetValueComparer(ValueCompareHelper.GUIDArrayCompare);
        }
    }
}
