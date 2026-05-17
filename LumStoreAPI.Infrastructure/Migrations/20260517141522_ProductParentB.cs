using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductParentB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                table: "ProductVariants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentQty",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ParentId",
                table: "ProductVariants",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_ProductVariants_ParentId",
                table: "ProductVariants",
                column: "ParentId",
                principalTable: "ProductVariants",
                principalColumn: "ItemID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_ProductVariants_ParentId",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ParentId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ParentQty",
                table: "Products");
        }
    }
}
