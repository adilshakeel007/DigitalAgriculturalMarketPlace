using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalAgriculturalMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class Phase5BulkRfqsAndQuotationAllocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rfqs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    DeliveryLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    RequiredByDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rfqs", x => x.Id);
                    table.CheckConstraint("CK_Rfqs_Status", "[Status] IN ('Draft','Open','PartiallyFulfilled','Fulfilled','Closed','Cancelled')");
                    table.ForeignKey(
                        name: "FK_Rfqs_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    QuotationNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ValidUntil = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.CheckConstraint("CK_Quotations_Status", "[Status] IN ('Draft','Submitted','PartiallyAccepted','Accepted','Rejected','Withdrawn','Expired')");
                    table.ForeignKey(
                        name: "FK_Quotations_Rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "Rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RfqItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    CatalogProductId = table.Column<int>(type: "int", nullable: false),
                    UnitOfMeasureId = table.Column<int>(type: "int", nullable: false),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    MaximumBudgetPerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Specifications = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RfqItems", x => x.Id);
                    table.CheckConstraint("CK_RfqItems_AcceptedQuantity", "[AcceptedQuantity] >= 0 AND [AcceptedQuantity] <= [RequestedQuantity]");
                    table.CheckConstraint("CK_RfqItems_RequestedQuantity", "[RequestedQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_RfqItems_CatalogProducts_CatalogProductId",
                        column: x => x.CatalogProductId,
                        principalTable: "CatalogProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RfqItems_Rfqs_RfqId",
                        column: x => x.RfqId,
                        principalTable: "Rfqs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RfqItems_UnitsOfMeasure_UnitOfMeasureId",
                        column: x => x.UnitOfMeasureId,
                        principalTable: "UnitsOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    RfqItemId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OfferedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationItems", x => x.Id);
                    table.CheckConstraint("CK_QuotationItems_AcceptedQuantity", "[AcceptedQuantity] >= 0 AND [AcceptedQuantity] <= [OfferedQuantity]");
                    table.CheckConstraint("CK_QuotationItems_OfferedQuantity", "[OfferedQuantity] > 0");
                    table.CheckConstraint("CK_QuotationItems_UnitPrice", "[UnitPrice] > 0");
                    table.ForeignKey(
                        name: "FK_QuotationItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationItems_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationItems_RfqItems_RfqItemId",
                        column: x => x.RfqItemId,
                        principalTable: "RfqItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationAllocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationItemId = table.Column<int>(type: "int", nullable: false),
                    RfqItemId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AcceptedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperationKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationAllocations", x => x.Id);
                    table.CheckConstraint("CK_QuotationAllocations_Price", "[AcceptedUnitPrice] > 0");
                    table.CheckConstraint("CK_QuotationAllocations_Quantity", "[AcceptedQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_QuotationAllocations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationAllocations_QuotationItems_QuotationItemId",
                        column: x => x.QuotationItemId,
                        principalTable: "QuotationItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationAllocations_RfqItems_RfqItemId",
                        column: x => x.RfqItemId,
                        principalTable: "RfqItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationAllocations_Users_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationAllocations_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAllocations_BuyerId",
                table: "QuotationAllocations",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAllocations_OperationKey",
                table: "QuotationAllocations",
                column: "OperationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAllocations_OrderId",
                table: "QuotationAllocations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAllocations_QuotationItemId",
                table: "QuotationAllocations",
                column: "QuotationItemId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAllocations_RfqItemId_CreatedAt",
                table: "QuotationAllocations",
                columns: new[] { "RfqItemId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationAllocations_SellerId",
                table: "QuotationAllocations",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_ProductId",
                table: "QuotationItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_QuotationId_RfqItemId",
                table: "QuotationItems",
                columns: new[] { "QuotationId", "RfqItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationItems_RfqItemId",
                table: "QuotationItems",
                column: "RfqItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_QuotationNumber",
                table: "Quotations",
                column: "QuotationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_RfqId_SellerId",
                table: "Quotations",
                columns: new[] { "RfqId", "SellerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_SellerId",
                table: "Quotations",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqItems_CatalogProductId",
                table: "RfqItems",
                column: "CatalogProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqItems_RfqId_CatalogProductId_UnitOfMeasureId",
                table: "RfqItems",
                columns: new[] { "RfqId", "CatalogProductId", "UnitOfMeasureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RfqItems_UnitOfMeasureId",
                table: "RfqItems",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Rfqs_BuyerId_Status",
                table: "Rfqs",
                columns: new[] { "BuyerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Rfqs_RfqNumber",
                table: "Rfqs",
                column: "RfqNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationAllocations");

            migrationBuilder.DropTable(
                name: "QuotationItems");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "RfqItems");

            migrationBuilder.DropTable(
                name: "Rfqs");
        }
    }
}
