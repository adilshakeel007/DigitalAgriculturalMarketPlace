using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalAgriculturalMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class Phase3UnifiedAccountsAndCatalogGovernance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "CatalogProducts",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "CatalogProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "CatalogProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CatalogProducts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                table: "CatalogProducts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedByUserId",
                table: "CatalogProducts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_ReviewedByUserId",
                table: "CatalogProducts",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_SubmittedByUserId_Name_Category_DefaultUnitId_Status",
                table: "CatalogProducts",
                columns: new[] { "SubmittedByUserId", "Name", "Category", "DefaultUnitId", "Status" },
                unique: true,
                filter: "[SubmittedByUserId] IS NOT NULL AND [DefaultUnitId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CatalogProducts_Status",
                table: "CatalogProducts",
                sql: "[Status] IN ('Pending', 'Approved', 'Rejected')");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogProducts_Users_ReviewedByUserId",
                table: "CatalogProducts",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogProducts_Users_SubmittedByUserId",
                table: "CatalogProducts",
                column: "SubmittedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(
                """
                INSERT INTO [SellerProfiles]
                    ([UserId], [DisplayName], [Description], [IsActive],
                     [VerificationStatus], [CreatedAt], [UpdatedAt])
                SELECT
                    [u].[Id], LEFT([u].[Name], 150), NULL, [u].[IsActive],
                    N'Unverified', SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM [Users] AS [u]
                WHERE [u].[UserType] = N'Farmer'
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [SellerProfiles] AS [s]
                      WHERE [s].[UserId] = [u].[Id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogProducts_Users_ReviewedByUserId",
                table: "CatalogProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_CatalogProducts_Users_SubmittedByUserId",
                table: "CatalogProducts");

            migrationBuilder.DropIndex(
                name: "IX_CatalogProducts_ReviewedByUserId",
                table: "CatalogProducts");

            migrationBuilder.DropIndex(
                name: "IX_CatalogProducts_SubmittedByUserId_Name_Category_DefaultUnitId_Status",
                table: "CatalogProducts");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CatalogProducts_Status",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                table: "CatalogProducts");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                table: "CatalogProducts");
        }
    }
}
