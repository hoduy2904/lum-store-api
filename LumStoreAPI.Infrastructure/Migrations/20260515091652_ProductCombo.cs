using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductCombo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCombo",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ProductCombos",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    VariantID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCombos", x => new { x.ProductID, x.VariantID });
                    table.ForeignKey(
                        name: "FK_ProductCombos_ProductVariants_VariantID",
                        column: x => x.VariantID,
                        principalTable: "ProductVariants",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCombos_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "PageID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCombos_VariantID",
                table: "ProductCombos",
                column: "VariantID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCombos");

            migrationBuilder.DropColumn(
                name: "IsCombo",
                table: "Products");
        }
    }
}
