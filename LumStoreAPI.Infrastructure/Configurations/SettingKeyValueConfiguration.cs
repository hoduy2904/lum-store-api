using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class SettingKeyValueConfiguration : IEntityTypeConfiguration<SettingKeyValue>
    {
        public void Configure(EntityTypeBuilder<SettingKeyValue> builder)
        {
            builder.HasKey(x => x.SettingCode);

            builder.Property(x => x.SettingCode)
                .HasMaxLength(60);
            builder.Property(x => x.SettingName)
                .HasMaxLength(60);

            builder.Property(x => x.SettingValue);
        }
    }
}
