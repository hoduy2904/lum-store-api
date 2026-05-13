using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeColorLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "ProductVariants");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "ProductVariants",
                type: "int",
                nullable: true,
                defaultValue: null);

            migrationBuilder.CreateTable(
                name: "ColorCategories",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    CategoryName = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    ItemOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorCategories", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "ColorItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    ItemOrder = table.Column<int>(type: "int", nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ColorValue = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColorItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_ColorItems_ColorCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ColorCategories",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ColorId",
                table: "ProductVariants",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorCategories_CategoryName",
                table: "ColorCategories",
                column: "CategoryName",
                unique: true,
                filter: "[CategoryName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ColorItems_CategoryId",
                table: "ColorItems",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ColorItems_ColorName",
                table: "ColorItems",
                column: "ColorName",
                unique: true,
                filter: "[ColorName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ColorItems_ColorValue",
                table: "ColorItems",
                column: "ColorValue",
                unique: true,
                filter: "[ColorValue] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_ColorItems_ColorId",
                table: "ProductVariants",
                column: "ColorId",
                principalTable: "ColorItems",
                principalColumn: "ItemID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_ColorItems_ColorId",
                table: "ProductVariants");

            migrationBuilder.DropTable(
                name: "ColorItems");

            migrationBuilder.DropTable(
                name: "ColorCategories");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ColorId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "ProductVariants");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "ProductVariants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
