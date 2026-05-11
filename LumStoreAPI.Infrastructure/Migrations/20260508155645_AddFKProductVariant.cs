using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFKProductVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the wrong FK if it somehow exists (defensive — FixProductVariantFKToNodeID already dropped it)
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_ProductVariants_Products_ProductID'
                )
                ALTER TABLE [ProductVariants] DROP CONSTRAINT [FK_ProductVariants_Products_ProductID];
            ");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductID",
                table: "ProductVariants",
                column: "ProductID");

            // ProductVariant.ProductID stores DocumentNode.NodeID, not Product.PageID.
            // Add an alternate key on DocumentPages.NodeID so EF can enforce the relationship correctly.
            migrationBuilder.AddUniqueConstraint(
                name: "AK_DocumentPages_NodeID",
                table: "DocumentPages",
                column: "NodeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_DocumentPages_ProductID",
                table: "ProductVariants",
                column: "ProductID",
                principalTable: "DocumentPages",
                principalColumn: "NodeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_DocumentPages_ProductID",
                table: "ProductVariants");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_DocumentPages_NodeID",
                table: "DocumentPages");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductID",
                table: "ProductVariants");
        }
    }
}
