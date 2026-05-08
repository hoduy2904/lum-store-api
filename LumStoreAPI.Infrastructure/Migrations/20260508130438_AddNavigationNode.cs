using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNavigationNode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEnableNavigation",
                table: "DocumentPages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OgDescription",
                table: "DocumentPages",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgImage",
                table: "DocumentPages",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OgTitle",
                table: "DocumentPages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnableNavigation",
                table: "DocumentPages");

            migrationBuilder.DropColumn(
                name: "OgDescription",
                table: "DocumentPages");

            migrationBuilder.DropColumn(
                name: "OgImage",
                table: "DocumentPages");

            migrationBuilder.DropColumn(
                name: "OgTitle",
                table: "DocumentPages");
        }
    }
}
