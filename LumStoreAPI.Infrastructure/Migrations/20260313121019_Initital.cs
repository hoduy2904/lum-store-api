using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initital : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentNodes",
                columns: table => new
                {
                    NodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentNodeID = table.Column<int>(type: "int", nullable: true),
                    NodeAlias = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NodeOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNodes", x => x.NodeID);
                    table.ForeignKey(
                        name: "FK_DocumentNodes_DocumentNodes_ParentNodeID",
                        column: x => x.ParentNodeID,
                        principalTable: "DocumentNodes",
                        principalColumn: "NodeID");
                });

            migrationBuilder.CreateTable(
                name: "EmailQueues",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailStatus = table.Column<int>(type: "int", nullable: false),
                    EmailFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailTo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailSubject = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: false),
                    EmailBcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailCc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventLogType = table.Column<int>(type: "int", nullable: false),
                    EventSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EventUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "MediaLibraryCategories",
                columns: table => new
                {
                    CategoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FolderName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaLibraryCategories", x => x.CategoryID);
                });

            migrationBuilder.CreateTable(
                name: "SettingKeyValues",
                columns: table => new
                {
                    SettingCode = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SettingName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingKeyValues", x => x.SettingCode);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserPassword = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TimeLocked = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserLevel = table.Column<int>(type: "int", nullable: false),
                    VerifyCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    TimeActionCode = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "DocumentLinkedNodes",
                columns: table => new
                {
                    Ancestor = table.Column<int>(type: "int", nullable: false),
                    Descendant = table.Column<int>(type: "int", nullable: false),
                    Depth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentLinkedNodes", x => new { x.Ancestor, x.Descendant, x.Depth });
                    table.ForeignKey(
                        name: "FK_DocumentLinkedNodes_DocumentNodes_Ancestor",
                        column: x => x.Ancestor,
                        principalTable: "DocumentNodes",
                        principalColumn: "NodeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DocumentLinkedNodes_DocumentNodes_Descendant",
                        column: x => x.Descendant,
                        principalTable: "DocumentNodes",
                        principalColumn: "NodeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPages",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NodeID = table.Column<int>(type: "int", nullable: false),
                    RequireAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PublishedFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPages", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_DocumentPages_DocumentNodes_NodeID",
                        column: x => x.NodeID,
                        principalTable: "DocumentNodes",
                        principalColumn: "NodeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MediaLibraries",
                columns: table => new
                {
                    FileID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CategoryID = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaLibraries", x => x.FileID);
                    table.ForeignKey(
                        name: "FK_MediaLibraries_MediaLibraryCategories_CategoryID",
                        column: x => x.CategoryID,
                        principalTable: "MediaLibraryCategories",
                        principalColumn: "CategoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    TokenID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.TokenID);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HomePages",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomePages", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_HomePages_DocumentPages_PageID",
                        column: x => x.PageID,
                        principalTable: "DocumentPages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_ProductCategories_DocumentPages_PageID",
                        column: x => x.PageID,
                        principalTable: "DocumentPages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UPC = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: true),
                    Length = table.Column<double>(type: "float", nullable: false),
                    Width = table.Column<double>(type: "float", nullable: false),
                    Height = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_Products_DocumentPages_PageID",
                        column: x => x.PageID,
                        principalTable: "DocumentPages",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLinkedNodes_Descendant",
                table: "DocumentLinkedNodes",
                column: "Descendant");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNodes_NodeOrder_ParentNodeID",
                table: "DocumentNodes",
                columns: new[] { "NodeOrder", "ParentNodeID" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNodes_ParentNodeID_NodeAlias",
                table: "DocumentNodes",
                columns: new[] { "ParentNodeID", "NodeAlias" },
                unique: true,
                filter: "[ParentNodeID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentPages_NodeID",
                table: "DocumentPages",
                column: "NodeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_EmailSubject",
                table: "EmailQueues",
                column: "EmailSubject");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_EmailTo",
                table: "EmailQueues",
                column: "EmailTo");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_EventCode",
                table: "EventLogs",
                column: "EventCode");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_EventName",
                table: "EventLogs",
                column: "EventName");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_EventSource",
                table: "EventLogs",
                column: "EventSource");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_IPAddress",
                table: "EventLogs",
                column: "IPAddress");

            migrationBuilder.CreateIndex(
                name: "IX_MediaLibraries_CategoryID",
                table: "MediaLibraries",
                column: "CategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_MediaLibraries_FileName",
                table: "MediaLibraries",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaLibraryCategories_CategoryName",
                table: "MediaLibraryCategories",
                column: "CategoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaLibraryCategories_FolderName",
                table: "MediaLibraryCategories",
                column: "FolderName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_Products_SKU",
                table: "Products",
                column: "SKU",
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UPC",
                table: "Products",
                column: "UPC");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserID",
                table: "UserTokens",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentLinkedNodes");

            migrationBuilder.DropTable(
                name: "EmailQueues");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "HomePages");

            migrationBuilder.DropTable(
                name: "MediaLibraries");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SettingKeyValues");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "MediaLibraryCategories");

            migrationBuilder.DropTable(
                name: "DocumentPages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DocumentNodes");
        }
    }
}
