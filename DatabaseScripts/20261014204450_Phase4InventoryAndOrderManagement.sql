BEGIN TRANSACTION;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET ARITHABORT ON;
SET NUMERIC_ROUNDABORT OFF;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [CancellationReason] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [CancelledAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [CompletedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [OrderNumber] nvarchar(40) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [SellerId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [Subtotal] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD [UpdatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE TABLE [InventoryMovements] (
        [Id] bigint NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [MovementType] nvarchar(40) NOT NULL,
        [AvailableDelta] decimal(18,3) NOT NULL,
        [ReservedDelta] decimal(18,3) NOT NULL,
        [AvailableAfter] decimal(18,3) NOT NULL,
        [ReservedAfter] decimal(18,3) NOT NULL,
        [ReferenceType] nvarchar(40) NULL,
        [ReferenceId] int NULL,
        [PerformedByUserId] int NULL,
        [Reason] nvarchar(500) NULL,
        [OperationKey] nvarchar(150) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_InventoryMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_InventoryMovements_AvailableAfter] CHECK ([AvailableAfter] >= 0),
        CONSTRAINT [CK_InventoryMovements_ReservedAfter] CHECK ([ReservedAfter] >= 0),
        CONSTRAINT [FK_InventoryMovements_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryMovements_Users_PerformedByUserId] FOREIGN KEY ([PerformedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE TABLE [OrderItems] (
        [Id] bigint NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [ProductId] int NOT NULL,
        [ProductNameSnapshot] nvarchar(150) NOT NULL,
        [UnitNameSnapshot] nvarchar(100) NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LineTotal] decimal(18,2) NOT NULL,
        [SellerId] int NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderItems_Quantity_Positive] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderItems_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE TABLE [OrderStatusHistory] (
        [Id] bigint NOT NULL IDENTITY,
        [OrderId] int NOT NULL,
        [FromStatus] nvarchar(30) NULL,
        [ToStatus] nvarchar(30) NOT NULL,
        [ChangedByUserId] int NULL,
        [Reason] nvarchar(500) NULL,
        [ChangedAt] datetime2 NOT NULL,
        [OperationKey] nvarchar(150) NOT NULL,
        CONSTRAINT [PK_OrderStatusHistory] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrderStatusHistory_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_OrderStatusHistory_Users_ChangedByUserId] FOREIGN KEY ([ChangedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE TABLE [InventoryReservations] (
        [Id] bigint NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [OrderItemId] bigint NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ReservedAt] datetime2 NOT NULL,
        [ReleasedAt] datetime2 NULL,
        [ConsumedAt] datetime2 NULL,
        [OperationKey] nvarchar(150) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_InventoryReservations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_InventoryReservations_Quantity_Positive] CHECK ([Quantity] > 0),
        CONSTRAINT [CK_InventoryReservations_Status] CHECK ([Status] IN ('Active','Released','Consumed')),
        CONSTRAINT [CK_InventoryReservations_TerminalState] CHECK (NOT ([ReleasedAt] IS NOT NULL AND [ConsumedAt] IS NOT NULL)),
        CONSTRAINT [FK_InventoryReservations_OrderItems_OrderItemId] FOREIGN KEY ([OrderItemId]) REFERENCES [OrderItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InventoryReservations_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]) WHERE [OrderNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_Orders_SellerId_Status] ON [Orders] ([SellerId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryMovements_OperationKey] ON [InventoryMovements] ([OperationKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_InventoryMovements_PerformedByUserId] ON [InventoryMovements] ([PerformedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_InventoryMovements_ProductId_CreatedAt] ON [InventoryMovements] ([ProductId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_InventoryReservations_OperationKey] ON [InventoryReservations] ([OperationKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InventoryReservations_OrderItemId] ON [InventoryReservations] ([OrderItemId]) WHERE [OrderItemId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_InventoryReservations_ProductId_Status] ON [InventoryReservations] ([ProductId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_OrderItems_SellerId] ON [OrderItems] ([SellerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_OrderStatusHistory_ChangedByUserId] ON [OrderStatusHistory] ([ChangedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_OrderStatusHistory_OperationKey] ON [OrderStatusHistory] ([OperationKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    CREATE INDEX [IX_OrderStatusHistory_OrderId_ChangedAt] ON [OrderStatusHistory] ([OrderId], [ChangedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014204450_Phase4InventoryAndOrderManagement'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014204450_Phase4InventoryAndOrderManagement', N'8.0.0');
END;
GO

COMMIT;
GO

