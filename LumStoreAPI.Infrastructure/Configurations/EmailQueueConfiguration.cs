using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
    {
        private const char SPLIT_CHAR = ',';

        public void Configure(EntityTypeBuilder<EmailQueue> builder)
        {
            var stringArrayComparer = new ValueComparer<string[]>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToArray()
            );

            builder.Property(x => x.EmailBody)
                .HasMaxLength(-1);

            builder.HasIndex(x => x.EmailSubject);
            builder.HasIndex(x => x.EmailTo);

            builder.Property(x => x.EmailTo)
                .HasConversion(
                x => string.Join(SPLIT_CHAR, x),
                x => x.Split(SPLIT_CHAR))
                .Metadata.SetValueComparer(stringArrayComparer);

            builder.Property(x => x.EmailCc)
                .HasConversion(
                x => x == null ? null : string.Join(SPLIT_CHAR, x),
                x => x == null ? null : x.Split(SPLIT_CHAR))
                .Metadata.SetValueComparer(stringArrayComparer);

            builder.Property(x => x.EmailBcc)
                .HasConversion(
                x => x == null ? null : string.Join(SPLIT_CHAR, x),
                x => x == null ? null : x.Split(SPLIT_CHAR))
                .Metadata.SetValueComparer(stringArrayComparer);

            builder.Property(x => x.Attachments)
                .HasConversion(
                x => x == null ? null : string.Join(SPLIT_CHAR, x),
                x => x == null ? null : x.Split(SPLIT_CHAR))
                .Metadata.SetValueComparer(stringArrayComparer);
        }
    }
}
