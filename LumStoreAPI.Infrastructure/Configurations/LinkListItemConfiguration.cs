using LumStoreAPI.Core.Entities.DocumentTypes;
using LumStoreAPI.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

public class LinkListItemConfiguration : IEntityTypeConfiguration<LinkListItem>
{
    public void Configure(EntityTypeBuilder<LinkListItem> builder)
    {
        builder.Property(x => x.LinkListTitle)
        .HasMaxLength(50);

        builder.Property(x => x.LinkListIcon)
            .HasMaxLength(40)
            .HasConversion(ConverterHelper.ArrayGuidConverter(','))
            .Metadata.SetValueComparer(ValueCompareHelper.GUIDArrayCompare);

        builder.Property(x => x.LinkUrl)
            .HasMaxLength(100);
    }
}
