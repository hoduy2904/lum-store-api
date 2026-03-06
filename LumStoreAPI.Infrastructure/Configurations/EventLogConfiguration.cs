using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
    {
        public void Configure(EntityTypeBuilder<EventLog> builder)
        {
            builder.HasKey(x => x.ItemID);

            builder.HasIndex(x => x.EventCode);
            builder.HasIndex(x => x.EventSource);
            builder.HasIndex(x => x.EventName);
            builder.HasIndex(x => x.IPAddress);

            builder.Property(x => x.EventSource)
                .HasMaxLength(100);
            builder.Property(x => x.EventName)
                .HasMaxLength(100);

            builder.Property(x => x.EventCode)
                .HasMaxLength(100);

            builder.Property(x => x.IPAddress)
                .HasMaxLength(30);

            builder.Property(x => x.EventDescription)
                .HasMaxLength(-1);
            builder.Property(x => x.ServerName)
                .HasMaxLength(100);

        }
    }
}
