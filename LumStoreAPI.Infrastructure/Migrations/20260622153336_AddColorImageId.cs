using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddColorImageId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ColorItems_ColorValue",
                table: "ColorItems");

            migrationBuilder.AlterColumn<string>(
                name: "ColorValue",
                table: "ColorItems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<Guid>(
                name: "ColorImageId",
                table: "ColorItems",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColorImageId",
                table: "ColorItems");

            migrationBuilder.AlterColumn<string>(
                name: "ColorValue",
                table: "ColorItems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ColorItems_ColorValue",
                table: "ColorItems",
                column: "ColorValue",
                unique: true);
        }
    }
}
