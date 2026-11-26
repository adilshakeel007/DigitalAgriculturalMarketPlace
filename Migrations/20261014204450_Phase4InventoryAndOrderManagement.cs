using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalAgriculturalMarketPlace.Migrations
{
    /// <inheritdoc />
    public partial class Phase4InventoryAndOrderManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "Orders",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Orders",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "SellerId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryMovements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    AvailableDelta = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReservedDelta = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AvailableAfter = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReservedAfter = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    PerformedByUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OperationKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryMovements", x => x.Id);
                    table.CheckConstraint("CK_InventoryMovements_AvailableAfter", "[AvailableAfter] >= 0");
                    table.CheckConstraint("CK_InventoryMovements_ReservedAfter", "[ReservedAfter] >= 0");
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryMovements_Users_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellerId = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.CheckConstraint("CK_OrderItems_Quantity_Positive", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderItems_Users_SellerId",
                        column: x => x.SellerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OperationKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistory_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryReservations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    OrderItemId = table.Column<long>(type: "bigint", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperationKey = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryReservations", x => x.Id);
                    table.CheckConstraint("CK_InventoryReservations_Quantity_Positive", "[Quantity] > 0");
                    table.CheckConstraint("CK_InventoryReservations_Status", "[Status] IN ('Active','Released','Consumed')");
                    table.CheckConstraint("CK_InventoryReservations_TerminalState", "NOT ([ReleasedAt] IS NOT NULL AND [ConsumedAt] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_InventoryReservations_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InventoryReservations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(
                """
                UPDATE o SET
                    SellerId = p.FarmerId,
                    OrderNumber = CONCAT(N'AG-LEGACY-', RIGHT(REPLICATE('0', 8) + CONVERT(varchar(20), o.Id), 8)),
                    Subtotal = o.TotalPrice,
                    CancelledAt = CASE WHEN o.Status = N'Cancelled' THEN o.OrderDate END,
                    CompletedAt = CASE WHEN o.Status = N'Completed' THEN o.OrderDate END,
                    UpdatedAt = o.OrderDate
                FROM Orders o INNER JOIN Products p ON p.Id = o.ProductId;

                INSERT INTO OrderItems (OrderId, ProductId, ProductNameSnapshot, UnitNameSnapshot, Quantity, UnitPrice, LineTotal, SellerId)
                SELECT o.Id, o.ProductId, p.Name, u.Name, CONVERT(decimal(18,3), o.Quantity),
                       CASE WHEN o.Quantity = 0 THEN o.TotalPrice ELSE CONVERT(decimal(18,2), o.TotalPrice / o.Quantity) END,
                       o.TotalPrice, p.FarmerId
                FROM Orders o INNER JOIN Products p ON p.Id=o.ProductId
                LEFT JOIN UnitsOfMeasure u ON u.Id=p.UnitOfMeasureId
                WHERE NOT EXISTS (SELECT 1 FROM OrderItems i WHERE i.OrderId=o.Id);

                INSERT INTO OrderStatusHistory (OrderId, FromStatus, ToStatus, ChangedByUserId, Reason, ChangedAt, OperationKey)
                SELECT o.Id, NULL, o.Status, NULL, N'Legacy current status imported; earlier transitions were not fabricated.', o.OrderDate,
                       CONCAT(N'legacy:status:',o.Id)
                FROM Orders o WHERE NOT EXISTS (SELECT 1 FROM OrderStatusHistory h WHERE h.OperationKey=CONCAT(N'legacy:status:',o.Id));

                UPDATE p SET AvailableQuantity=CONVERT(decimal(18,3),p.Quantity),
                    ReservedQuantity=COALESCE(a.ActiveQuantity,0)
                FROM Products p OUTER APPLY (
                    SELECT SUM(CONVERT(decimal(18,3),o.Quantity)) ActiveQuantity FROM Orders o
                    WHERE o.ProductId=p.Id AND o.Status IN (N'Pending',N'Confirmed')) a;

                INSERT INTO InventoryReservations (ProductId,OrderItemId,Quantity,Status,ReservedAt,OperationKey)
                SELECT o.ProductId,i.Id,i.Quantity,N'Active',o.OrderDate,CONCAT(N'legacy:reservation:',o.Id)
                FROM Orders o INNER JOIN OrderItems i ON i.OrderId=o.Id
                WHERE o.Status IN (N'Pending',N'Confirmed')
                  AND NOT EXISTS (SELECT 1 FROM InventoryReservations r WHERE r.OperationKey=CONCAT(N'legacy:reservation:',o.Id));

                INSERT INTO InventoryMovements (ProductId,MovementType,AvailableDelta,ReservedDelta,AvailableAfter,ReservedAfter,ReferenceType,ReferenceId,PerformedByUserId,Reason,OperationKey,CreatedAt)
                SELECT p.Id,N'OpeningBalance',p.AvailableQuantity,p.ReservedQuantity,p.AvailableQuantity,p.ReservedQuantity,N'Product',p.Id,NULL,
                       N'Phase 4 baseline: legacy Quantity retained as available; active previously deducted orders reconstructed as reserved.',
                       CONCAT(N'legacy:opening:',p.Id),SYSUTCDATETIME()
                FROM Products p WHERE NOT EXISTS (SELECT 1 FROM InventoryMovements m WHERE m.OperationKey=CONCAT(N'legacy:opening:',p.Id));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true,
                filter: "[OrderNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SellerId_Status",
                table: "Orders",
                columns: new[] { "SellerId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_OperationKey",
                table: "InventoryMovements",
                column: "OperationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_PerformedByUserId",
                table: "InventoryMovements",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryMovements_ProductId_CreatedAt",
                table: "InventoryMovements",
                columns: new[] { "ProductId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_OperationKey",
                table: "InventoryReservations",
                column: "OperationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_OrderItemId",
                table: "InventoryReservations",
                column: "OrderItemId",
                unique: true,
                filter: "[OrderItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryReservations_ProductId_Status",
                table: "InventoryReservations",
                columns: new[] { "ProductId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_SellerId",
                table: "OrderItems",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_ChangedByUserId",
                table: "OrderStatusHistory",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OperationKey",
                table: "OrderStatusHistory",
                column: "OperationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistory_OrderId_ChangedAt",
                table: "OrderStatusHistory",
                columns: new[] { "OrderId", "ChangedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_SellerId",
                table: "Orders",
                column: "SellerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_SellerId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "InventoryMovements");

            migrationBuilder.DropTable(
                name: "InventoryReservations");

            migrationBuilder.DropTable(
                name: "OrderStatusHistory");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SellerId_Status",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SellerId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Orders");
        }
    }
}
