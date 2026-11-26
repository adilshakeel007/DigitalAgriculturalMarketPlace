using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DigitalAgriculturalMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class Phase2CompatibilityFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLoginAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Users",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableQuantity",
                table: "Products",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CatalogProductId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumOrderQuantity",
                table: "Products",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedQuantity",
                table: "Products",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    VerificationStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.UserId);
                    table.CheckConstraint("CK_SellerProfiles_VerificationStatus", "[VerificationStatus] IN ('Unverified', 'Verified', 'Rejected')");
                    table.ForeignKey(
                        name: "FK_SellerProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitsOfMeasure",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DecimalPlaces = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitsOfMeasure", x => x.Id);
                    table.CheckConstraint("CK_UnitsOfMeasure_DecimalPlaces", "[DecimalPlaces] BETWEEN 0 AND 3");
                });

            migrationBuilder.CreateTable(
                name: "CatalogProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DefaultUnitId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogProducts_UnitsOfMeasure_DefaultUnitId",
                        column: x => x.DefaultUnitId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "UnitsOfMeasure",
                columns: new[] { "Id", "Code", "DecimalPlaces", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "kg", (byte)3, true, "Kilogram" },
                    { 2, "tonne", (byte)3, true, "Tonne" },
                    { 3, "bag", (byte)0, true, "Bag" },
                    { 4, "crate", (byte)0, true, "Crate" },
                    { 5, "piece", (byte)0, true, "Piece" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CatalogProductId_IsActive",
                table: "Products",
                columns: new[] { "CatalogProductId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_FarmerId_IsActive",
                table: "Products",
                columns: new[] { "FarmerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_AvailableQuantity_NonNegative",
                table: "Products",
                sql: "[AvailableQuantity] IS NULL OR [AvailableQuantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_MinimumOrderQuantity_Positive",
                table: "Products",
                sql: "[MinimumOrderQuantity] IS NULL OR [MinimumOrderQuantity] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Products_ReservedQuantity_NonNegative",
                table: "Products",
                sql: "[ReservedQuantity] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_Category_Name",
                table: "CatalogProducts",
                columns: new[] { "Category", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_DefaultUnitId",
                table: "CatalogProducts",
                column: "DefaultUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_Code",
                table: "UnitsOfMeasure",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_CatalogProducts_CatalogProductId",
                table: "Products",
                column: "CatalogProductId",
                principalTable: "CatalogProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
                table: "Products",
                column: "UnitOfMeasureId",
                principalTable: "UnitsOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_CatalogProducts_CatalogProductId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_UnitsOfMeasure_UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "CatalogProducts");

            migrationBuilder.DropTable(
                name: "SellerProfiles");

            migrationBuilder.DropTable(
                name: "UnitsOfMeasure");

            migrationBuilder.DropIndex(
                name: "IX_Products_CatalogProductId_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_FarmerId_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_AvailableQuantity_NonNegative",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_MinimumOrderQuantity_Positive",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Products_ReservedQuantity_NonNegative",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LastLoginAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CatalogProductId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinimumOrderQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ReservedQuantity",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Products");
        }
    }
}
