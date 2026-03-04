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
        }
    }
}
