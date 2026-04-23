using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserWishlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWishlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NodeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWishlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWishlists_DocumentNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "DocumentNodes",
                        principalColumn: "NodeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWishlists_NodeId",
                table: "UserWishlists",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWishlists_UserId",
                table: "UserWishlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWishlists_UserId_NodeId",
                table: "UserWishlists",
                columns: new[] { "UserId", "NodeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWishlists");
        }
    }
}
