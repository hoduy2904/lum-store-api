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
            migrationBuilder.CreateSequence(
                name: "BaseClassItemSequence");

            migrationBuilder.CreateTable(
                name: "BaseClassItem",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseClassItem", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "CustomerTiers",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    TierLevel = table.Column<int>(type: "int", nullable: false),
                    TierName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinPoints = table.Column<int>(type: "int", nullable: false),
                    MaxPoints = table.Column<int>(type: "int", nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PointsPerDollar = table.Column<int>(type: "int", nullable: false),
                    BadgeColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerTiers", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "DiscountRules",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    RuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MinQuantity = table.Column<int>(type: "int", nullable: false),
                    MaxQuantity = table.Column<int>(type: "int", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountRules", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "DocumentNodes",
                columns: table => new
                {
                    NodeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NodeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentNodeID = table.Column<int>(type: "int", nullable: true),
                    NodeAlias = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NodeOrder = table.Column<int>(type: "int", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelativeUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
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
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    EmailStatus = table.Column<int>(type: "int", nullable: false),
                    EmailFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailTo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailSubject = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: false),
                    EmailBcc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailCc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NextRetryTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "IntegrationConfigs",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    IntegrationType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ApiSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebhookSecret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdditionalConfig = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationConfigs", x => x.ItemID);
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
                name: "ShiprelayDataSyncs",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    VariantID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryActionStatus = table.Column<int>(type: "int", nullable: false),
                    RunnedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiprelayDataSyncs", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "SyncLogs",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    IntegrationType = table.Column<int>(type: "int", nullable: false),
                    SyncMode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorDetail = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: true),
                    RecordsSynced = table.Column<int>(type: "int", nullable: false),
                    RecordsFailed = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLogs", x => x.ItemID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
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
                    TimeLocked = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UserLevel = table.Column<int>(type: "int", nullable: false),
                    VerifyCode = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    TimeActionCode = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PublishedTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    PublishedFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DocumentPageWidgets = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
                name: "CustomerProfiles",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TierLevel = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    AvailablePoints = table.Column<int>(type: "int", nullable: false),
                    TotalSpent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Birthday = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerProfiles", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_CustomerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    EventLogType = table.Column<int>(type: "int", nullable: false),
                    EventSource = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EventDescription = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    EventUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_EventLogs_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "ItemID");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ShippingCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShippingZip = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShippingCountry = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShiprelayShipmentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShippingCarrier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ShippedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DeliveredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CustomerNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_Orders_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    TokenID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "HomePages",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarouselPathId = table.Column<int>(type: "int", nullable: false)
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
                name: "LinkLists",
                columns: table => new
                {
                    PageID = table.Column<int>(type: "int", nullable: false),
                    LinkListTitle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinkListIcon = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkLists", x => x.PageID);
                    table.ForeignKey(
                        name: "FK_LinkLists_DocumentPages_PageID",
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
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CategoryImage = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CategoryDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
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
                    ProductType = table.Column<int>(type: "int", nullable: false),
                    ProductGroup = table.Column<int>(type: "int", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: -1, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsBestSeller = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    ReviewCount = table.Column<int>(type: "int", nullable: false),
                    Length = table.Column<double>(type: "float", nullable: false),
                    Width = table.Column<double>(type: "float", nullable: false),
                    Height = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    IsFoldable = table.Column<bool>(type: "bit", nullable: false),
                    IsAlcoholic = table.Column<bool>(type: "bit", nullable: false),
                    IsHazmat = table.Column<bool>(type: "bit", nullable: false),
                    IsNeedBox = table.Column<bool>(type: "bit", nullable: false),
                    IsFragile = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "CustomerNotes",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    CustomerProfileId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotes", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_CustomerNotes_CustomerProfiles_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerNotes_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPoints",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    CustomerProfileId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPoints", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_LoyaltyPoints_CustomerProfiles_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfiles",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderHistories",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    ChangedByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsSystemAction = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderHistories", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_OrderHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VariantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderNotes",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderNotes", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_OrderNotes_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderNotes_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderReturns",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedByUserId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderReturns", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_OrderReturns_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderReturns_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    ProductID = table.Column<int>(type: "int", nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UPC = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VariantName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ShiprelayId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductID",
                        column: x => x.ProductID,
                        principalTable: "Products",
                        principalColumn: "PageID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderReturnItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [BaseClassItemSequence]"),
                    OrderReturnId = table.Column<int>(type: "int", nullable: false),
                    OrderItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderReturnItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_OrderReturnItems_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderReturnItems_OrderReturns_OrderReturnId",
                        column: x => x.OrderReturnId,
                        principalTable: "OrderReturns",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_AuthorId",
                table: "CustomerNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotes_CustomerProfileId",
                table: "CustomerNotes",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_TierLevel",
                table: "CustomerProfiles",
                column: "TierLevel");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfiles_UserId",
                table: "CustomerProfiles",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTiers_TierLevel",
                table: "CustomerTiers",
                column: "TierLevel",
                unique: true,
                filter: "[TierLevel] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountRules_IsActive",
                table: "DiscountRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentLinkedNodes_Descendant",
                table: "DocumentLinkedNodes",
                column: "Descendant");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNodes_ClassName",
                table: "DocumentNodes",
                column: "ClassName");

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
                name: "IX_DocumentNodes_RelativeUrl",
                table: "DocumentNodes",
                column: "RelativeUrl");

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
                name: "IX_EventLogs_UserID",
                table: "EventLogs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationConfigs_IntegrationType",
                table: "IntegrationConfigs",
                column: "IntegrationType");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPoints_CustomerProfileId",
                table: "LoyaltyPoints",
                column: "CustomerProfileId");

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
                name: "IX_OrderHistories_ChangedByUserId",
                table: "OrderHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderHistories_OrderId",
                table: "OrderHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_AuthorId",
                table: "OrderNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderNotes_OrderId",
                table: "OrderNotes",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturnItems_OrderItemId",
                table: "OrderReturnItems",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturnItems_OrderReturnId",
                table: "OrderReturnItems",
                column: "OrderReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturns_OrderId",
                table: "OrderReturns",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderReturns_ReviewedByUserId",
                table: "OrderReturns",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderCode",
                table: "Orders",
                column: "OrderCode",
                unique: true,
                filter: "[OrderCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductName",
                table: "Products",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductID",
                table: "ProductVariants",
                column: "ProductID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ShiprelayId",
                table: "ProductVariants",
                column: "ShiprelayId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_SKU",
                table: "ProductVariants",
                column: "SKU",
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_IntegrationType",
                table: "SyncLogs",
                column: "IntegrationType");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLogs_StartedAt",
                table: "SyncLogs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true,
                filter: "[UserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserID",
                table: "UserTokens",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BaseClassItem");

            migrationBuilder.DropTable(
                name: "CTAImages");

            migrationBuilder.DropTable(
                name: "CustomerNotes");

            migrationBuilder.DropTable(
                name: "CustomerTiers");

            migrationBuilder.DropTable(
                name: "DiscountRules");

            migrationBuilder.DropTable(
                name: "DocumentLinkedNodes");

            migrationBuilder.DropTable(
                name: "EmailQueues");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "HomePages");

            migrationBuilder.DropTable(
                name: "IntegrationConfigs");

            migrationBuilder.DropTable(
                name: "LinkLists");

            migrationBuilder.DropTable(
                name: "LoyaltyPoints");

            migrationBuilder.DropTable(
                name: "MediaLibraries");

            migrationBuilder.DropTable(
                name: "OrderHistories");

            migrationBuilder.DropTable(
                name: "OrderNotes");

            migrationBuilder.DropTable(
                name: "OrderReturnItems");

            migrationBuilder.DropTable(
                name: "ProductCategories");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "SettingKeyValues");

            migrationBuilder.DropTable(
                name: "ShiprelayDataSyncs");

            migrationBuilder.DropTable(
                name: "SyncLogs");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "CustomerProfiles");

            migrationBuilder.DropTable(
                name: "MediaLibraryCategories");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OrderReturns");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "DocumentPages");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "DocumentNodes");

            migrationBuilder.DropSequence(
                name: "BaseClassItemSequence");
        }
    }
}
