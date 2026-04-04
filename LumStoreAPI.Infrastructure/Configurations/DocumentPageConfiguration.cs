using System.Text.Json;
using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Libraries.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class DocumentPageConfiguration : IEntityTypeConfiguration<DocumentPage>
    {
        public void Configure(EntityTypeBuilder<DocumentPage> builder)
        {
            var converter = new ValueConverter<WidgetData<object>[], string>(
                v => JsonSerializer.Serialize(v),
                v =>
                    v.IsValidJson(true)
                    ? (JsonSerializer.Deserialize<WidgetData<object>[]>(v) ?? Array.Empty<WidgetData<object>>())
                    : Array.Empty<WidgetData<object>>()
            );
            builder.HasKey(x => x.PageID);

            builder.Property(x => x.DocumentName)
                .HasMaxLength(100);

            builder.Property(x => x.DocumentPageWidgets)
            .HasConversion(converter);

            builder.UseTptMappingStrategy();
        }
    }
}
