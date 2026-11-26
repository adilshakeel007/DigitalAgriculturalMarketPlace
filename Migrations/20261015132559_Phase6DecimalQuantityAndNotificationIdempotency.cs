using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalAgriculturalMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class Phase6DecimalQuantityAndNotificationIdempotency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Orders",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "OperationKey",
                table: "Notifications",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Orders_Quantity_Positive",
                table: "Orders",
                sql: "[Quantity] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_OperationKey",
                table: "Notifications",
                column: "OperationKey",
                unique: true,
                filter: "[OperationKey] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Orders_Quantity_Positive",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_OperationKey",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "OperationKey",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");
        }
    }
}
