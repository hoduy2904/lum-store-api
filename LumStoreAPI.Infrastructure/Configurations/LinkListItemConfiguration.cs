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
        .HasMaxLength(100);

        builder.Property(x => x.IconName)
            .HasMaxLength(40);

        builder.Property(x => x.LinkUrl)
            .HasMaxLength(250);
    }
}
