using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProductVariantFKToNodeID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the FK that wrongly referenced Products.PageID.
            // The DB may already have this dropped manually — guard with IF EXISTS.
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_ProductVariants_Products_ProductID'
                )
                ALTER TABLE [ProductVariants] DROP CONSTRAINT [FK_ProductVariants_Products_ProductID];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Products_ProductID",
                table: "ProductVariants",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "PageID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
