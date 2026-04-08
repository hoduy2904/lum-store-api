using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CTAImageItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HeroSlidesJson",
                table: "HomePages");

            migrationBuilder.AddColumn<int>(
                name: "CarouselPathId",
                table: "HomePages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CTAImages",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    Pretitle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Image = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PrimaryButton = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CTAImages", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_CTAImages_DocumentPages_PageID",
                        column: x => x.PageID,
                        principalTable: "DocumentPages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CTAImages");

            migrationBuilder.DropColumn(
                name: "CarouselPathId",
                table: "HomePages");

            migrationBuilder.AddColumn<string>(
                name: "HeroSlidesJson",
                table: "HomePages",
                type: "nvarchar(max)",
                maxLength: -1,
                nullable: true);
        }
    }
}
