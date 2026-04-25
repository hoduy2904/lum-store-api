using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderCustomerIdFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_Orders_Users_CustomerId'
                )
                BEGIN
                    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_Users_CustomerId];
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_Orders_CustomerProfiles_CustomerId'
                )
                BEGIN
                    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_CustomerProfiles_CustomerId]
                        FOREIGN KEY ([CustomerId]) REFERENCES [CustomerProfiles]([ItemID]) ON DELETE SET NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_Orders_CustomerProfiles_CustomerId'
                )
                BEGIN
                    ALTER TABLE [Orders] DROP CONSTRAINT [FK_Orders_CustomerProfiles_CustomerId];
                END
            ");
        }
    }
}
