BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE TABLE [Rfqs] (
        [Id] int NOT NULL IDENTITY,
        [RfqNumber] nvarchar(40) NOT NULL,
        [BuyerId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(2000) NULL,
        [DeliveryLocation] nvarchar(300) NOT NULL,
        [RequiredByDate] datetime2 NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [ClosedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Rfqs] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Rfqs_Status] CHECK ([Status] IN ('Draft','Open','PartiallyFulfilled','Fulfilled','Closed','Cancelled')),
        CONSTRAINT [FK_Rfqs_Users_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE TABLE [Quotations] (
        [Id] int NOT NULL IDENTITY,
        [RfqId] int NOT NULL,
        [SellerId] int NOT NULL,
        [QuotationNumber] nvarchar(40) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Notes] nvarchar(2000) NULL,
        [ValidUntil] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Quotations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Quotations_Status] CHECK ([Status] IN ('Draft','Submitted','PartiallyAccepted','Accepted','Rejected','Withdrawn','Expired')),
        CONSTRAINT [FK_Quotations_Rfqs_RfqId] FOREIGN KEY ([RfqId]) REFERENCES [Rfqs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Quotations_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE TABLE [RfqItems] (
        [Id] int NOT NULL IDENTITY,
        [RfqId] int NOT NULL,
        [CatalogProductId] int NOT NULL,
        [UnitOfMeasureId] int NOT NULL,
        [RequestedQuantity] decimal(18,3) NOT NULL,
        [AcceptedQuantity] decimal(18,3) NOT NULL,
        [MaximumBudgetPerUnit] decimal(18,2) NULL,
        [Specifications] nvarchar(1000) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RfqItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_RfqItems_AcceptedQuantity] CHECK ([AcceptedQuantity] >= 0 AND [AcceptedQuantity] <= [RequestedQuantity]),
        CONSTRAINT [CK_RfqItems_RequestedQuantity] CHECK ([RequestedQuantity] > 0),
        CONSTRAINT [FK_RfqItems_CatalogProducts_CatalogProductId] FOREIGN KEY ([CatalogProductId]) REFERENCES [CatalogProducts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RfqItems_Rfqs_RfqId] FOREIGN KEY ([RfqId]) REFERENCES [Rfqs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RfqItems_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE TABLE [QuotationItems] (
        [Id] int NOT NULL IDENTITY,
        [QuotationId] int NOT NULL,
        [RfqItemId] int NOT NULL,
        [ProductId] int NOT NULL,
        [OfferedQuantity] decimal(18,3) NOT NULL,
        [AcceptedQuantity] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [EstimatedDeliveryDate] datetime2 NOT NULL,
        [Notes] nvarchar(1000) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_QuotationItems] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_QuotationItems_AcceptedQuantity] CHECK ([AcceptedQuantity] >= 0 AND [AcceptedQuantity] <= [OfferedQuantity]),
        CONSTRAINT [CK_QuotationItems_OfferedQuantity] CHECK ([OfferedQuantity] > 0),
        CONSTRAINT [CK_QuotationItems_UnitPrice] CHECK ([UnitPrice] > 0),
        CONSTRAINT [FK_QuotationItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuotationItems_Quotations_QuotationId] FOREIGN KEY ([QuotationId]) REFERENCES [Quotations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuotationItems_RfqItems_RfqItemId] FOREIGN KEY ([RfqItemId]) REFERENCES [RfqItems] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE TABLE [QuotationAllocations] (
        [Id] bigint NOT NULL IDENTITY,
        [QuotationItemId] int NOT NULL,
        [RfqItemId] int NOT NULL,
        [BuyerId] int NOT NULL,
        [SellerId] int NOT NULL,
        [OrderId] int NOT NULL,
        [AcceptedQuantity] decimal(18,3) NOT NULL,
        [AcceptedUnitPrice] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [OperationKey] nvarchar(150) NOT NULL,
        CONSTRAINT [PK_QuotationAllocations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_QuotationAllocations_Price] CHECK ([AcceptedUnitPrice] > 0),
        CONSTRAINT [CK_QuotationAllocations_Quantity] CHECK ([AcceptedQuantity] > 0),
        CONSTRAINT [FK_QuotationAllocations_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuotationAllocations_QuotationItems_QuotationItemId] FOREIGN KEY ([QuotationItemId]) REFERENCES [QuotationItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuotationAllocations_RfqItems_RfqItemId] FOREIGN KEY ([RfqItemId]) REFERENCES [RfqItems] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuotationAllocations_Users_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_QuotationAllocations_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationAllocations_BuyerId] ON [QuotationAllocations] ([BuyerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QuotationAllocations_OperationKey] ON [QuotationAllocations] ([OperationKey]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationAllocations_OrderId] ON [QuotationAllocations] ([OrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationAllocations_QuotationItemId] ON [QuotationAllocations] ([QuotationItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationAllocations_RfqItemId_CreatedAt] ON [QuotationAllocations] ([RfqItemId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationAllocations_SellerId] ON [QuotationAllocations] ([SellerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationItems_ProductId] ON [QuotationItems] ([ProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_QuotationItems_QuotationId_RfqItemId] ON [QuotationItems] ([QuotationId], [RfqItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_QuotationItems_RfqItemId] ON [QuotationItems] ([RfqItemId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Quotations_QuotationNumber] ON [Quotations] ([QuotationNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Quotations_RfqId_SellerId] ON [Quotations] ([RfqId], [SellerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_Quotations_SellerId] ON [Quotations] ([SellerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_RfqItems_CatalogProductId] ON [RfqItems] ([CatalogProductId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RfqItems_RfqId_CatalogProductId_UnitOfMeasureId] ON [RfqItems] ([RfqId], [CatalogProductId], [UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_RfqItems_UnitOfMeasureId] ON [RfqItems] ([UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE INDEX [IX_Rfqs_BuyerId_Status] ON [Rfqs] ([BuyerId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rfqs_RfqNumber] ON [Rfqs] ([RfqNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014211537_Phase5BulkRfqsAndQuotationAllocations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014211537_Phase5BulkRfqsAndQuotationAllocations', N'8.0.0');
END;
GO

COMMIT;
GO

