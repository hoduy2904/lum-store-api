using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LumStoreAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentIntent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeRefundId",
                table: "OrderReturns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "StripeRefundId",
                table: "OrderReturns");
        }
    }
}
