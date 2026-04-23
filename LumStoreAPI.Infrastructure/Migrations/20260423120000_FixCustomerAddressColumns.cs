using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCustomerAddressColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename Street → Address (original migration used 'Street').
            // Guard with IF EXISTS so re-running after a manual rename is safe.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CustomerAddresses' AND COLUMN_NAME = 'Street'
                )
                EXEC sp_rename 'CustomerAddresses.Street', 'Address', 'COLUMN';
            ");

            // Drop CustomerId FK/index/column if they were manually added.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_CustomerAddresses_CustomerProfiles_CustomerId'
                )
                ALTER TABLE [CustomerAddresses] DROP CONSTRAINT [FK_CustomerAddresses_CustomerProfiles_CustomerId];

                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_CustomerAddresses_CustomerId' AND object_id = OBJECT_ID('CustomerAddresses')
                )
                DROP INDEX [IX_CustomerAddresses_CustomerId] ON [CustomerAddresses];

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CustomerAddresses' AND COLUMN_NAME = 'CustomerId'
                )
                ALTER TABLE [CustomerAddresses] DROP COLUMN [CustomerId];

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CustomerAddresses' AND COLUMN_NAME = 'ZipCode'
                )
                ALTER TABLE [CustomerAddresses] DROP COLUMN [ZipCode];

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CustomerAddresses' AND COLUMN_NAME = 'Country'
                )
                ALTER TABLE [CustomerAddresses] DROP COLUMN [Country];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CustomerAddresses' AND COLUMN_NAME = 'Address'
                )
                EXEC sp_rename 'CustomerAddresses.Address', 'Street', 'COLUMN';
            ");
        }
    }
}
