using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeneralContents",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descrition = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneralContents", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_GeneralContents_DocumentPages_PageID",
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
                name: "GeneralContents");
        }
    }
}
