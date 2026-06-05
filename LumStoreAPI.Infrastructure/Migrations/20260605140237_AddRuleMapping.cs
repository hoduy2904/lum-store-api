using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRuleMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "DiscountRules");

            migrationBuilder.CreateTable(
                name: "DiscountRuleMappings",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    DiscountRuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRuleMappings", x => new { x.ProductId, x.DiscountRuleId });
                    table.ForeignKey(
                        name: "FK_DiscountRuleMappings_DiscountRules_DiscountRuleId",
                        column: x => x.DiscountRuleId,
                        principalTable: "DiscountRules",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRuleMappings_DiscountRuleId",
                table: "DiscountRuleMappings",
                column: "DiscountRuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscountRuleMappings");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "DiscountRules",
                type: "int",
                nullable: true);
        }
    }
}
