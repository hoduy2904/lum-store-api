using LumStoreAPI.Core.Entities.Systems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LumStoreAPI.Infrastructure.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.ItemID);

            builder.HasIndex(x => x.UserName).IsUnique();

            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.Email)
                .HasMaxLength(30);

            builder.Property(x => x.UserPassword)
                .HasMaxLength(100);

            builder.Property(x => x.FirstName)
               .HasMaxLength(30);
            builder.Property(x => x.MiddleName)
               .HasMaxLength(50);
            builder.Property(x => x.LastName)
               .HasMaxLength(30);

            builder.Property(x => x.Avatar)
               .HasMaxLength(255);

            builder.Property(x => x.VerifyCode)
               .HasMaxLength(7);

            builder.HasMany(x => x.UserTokens)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserID);
        }
    }
}
