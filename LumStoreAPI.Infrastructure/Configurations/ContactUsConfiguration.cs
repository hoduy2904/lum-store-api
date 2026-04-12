using System;
using LumStoreAPI.Core.Entities.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations;

public class ContactUsConfiguration : IEntityTypeConfiguration<ContactUs>
{
    public void Configure(EntityTypeBuilder<ContactUs> builder)
    {
        builder.Property(x => x.Title)
            .HasMaxLength(100);

        builder.Property(x => x.Descrition)
        .HasMaxLength(200);
    }
}
