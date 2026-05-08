using LumStoreAPI.Core.Entities.DocumentEngine;
using LumStoreAPI.Core.Models.Systems;
using LumStoreAPI.Infrastructure.Helpers;
using LumStoreAPI.Libraries.Extensions;
using LumStoreAPI.Libraries.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class DocumentPageConfiguration : IEntityTypeConfiguration<DocumentPage>
    {
        private static WidgetData<object>[] ConvertToObject(string data)
        {
            if (!data.IsValidJson(true)) return [];
            return (JsonSerializer.Deserialize<WidgetData<object>[]>(data) ?? []).Select(x =>
            {
                if (DocumentPageTypeHelper.DocumentWidgets.TryGetValue(x.WidgetCode, out Type? widgetQuery) && x.Properties is JsonElement jsonElement)
                {
                    x.Properties = jsonElement.Deserialize(widgetQuery, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
                    });
                }
                return x;
            }).ToArray();
        }
        public void Configure(EntityTypeBuilder<DocumentPage> builder)
        {
            var converter = new ValueConverter<WidgetData<object>[], string>(
                v => JsonSerializer.Serialize(v),
                v => ConvertToObject(v)
            );

            builder.HasKey(x => x.PageID);

            builder.Property(x => x.DocumentName)
                .HasMaxLength(100);

            builder.Property(x => x.OgTitle).HasMaxLength(100);
            builder.Property(x => x.OgDescription).HasMaxLength(250);

            builder.Property(x => x.OgImage)
                .HasMaxLength(20)
                .HasConversion(ConverterHelper.ArrayGuidConverter(','))
                .Metadata.SetValueComparer(ValueCompareHelper.GUIDArrayCompare);

            builder.Property(x => x.DocumentPageWidgets)
            .HasConversion(converter);

            builder.UseTptMappingStrategy();
        }
    }
}
