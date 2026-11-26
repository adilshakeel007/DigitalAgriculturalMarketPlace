BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD [LastLoginAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Users] ADD [UpdatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [AvailableQuantity] decimal(18,3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [CatalogProductId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [MinimumOrderQuantity] decimal(18,3) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [ReservedQuantity] decimal(18,3) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [RowVersion] rowversion NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [UnitOfMeasureId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD [UpdatedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE TABLE [SellerProfiles] (
        [UserId] int NOT NULL,
        [DisplayName] nvarchar(150) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [IsActive] bit NOT NULL,
        [VerificationStatus] nvarchar(30) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_SellerProfiles] PRIMARY KEY ([UserId]),
        CONSTRAINT [CK_SellerProfiles_VerificationStatus] CHECK ([VerificationStatus] IN ('Unverified', 'Verified', 'Rejected')),
        CONSTRAINT [FK_SellerProfiles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE TABLE [UnitsOfMeasure] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [DecimalPlaces] tinyint NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_UnitsOfMeasure] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_UnitsOfMeasure_DecimalPlaces] CHECK ([DecimalPlaces] BETWEEN 0 AND 3)
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE TABLE [CatalogProducts] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(150) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [DefaultUnitId] int NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CatalogProducts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CatalogProducts_UnitsOfMeasure_DefaultUnitId] FOREIGN KEY ([DefaultUnitId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DecimalPlaces', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [UnitsOfMeasure] ON;
    EXEC(N'INSERT INTO [UnitsOfMeasure] ([Id], [Code], [DecimalPlaces], [IsActive], [Name])
    VALUES (1, N''kg'', CAST(3 AS tinyint), CAST(1 AS bit), N''Kilogram''),
    (2, N''tonne'', CAST(3 AS tinyint), CAST(1 AS bit), N''Tonne''),
    (3, N''bag'', CAST(0 AS tinyint), CAST(1 AS bit), N''Bag''),
    (4, N''crate'', CAST(0 AS tinyint), CAST(1 AS bit), N''Crate''),
    (5, N''piece'', CAST(0 AS tinyint), CAST(1 AS bit), N''Piece'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DecimalPlaces', N'IsActive', N'Name') AND [object_id] = OBJECT_ID(N'[UnitsOfMeasure]'))
        SET IDENTITY_INSERT [UnitsOfMeasure] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE INDEX [IX_Products_CatalogProductId_IsActive] ON [Products] ([CatalogProductId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE INDEX [IX_Products_FarmerId_IsActive] ON [Products] ([FarmerId], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE INDEX [IX_Products_UnitOfMeasureId] ON [Products] ([UnitOfMeasureId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    EXEC(N'ALTER TABLE [Products] ADD CONSTRAINT [CK_Products_AvailableQuantity_NonNegative] CHECK ([AvailableQuantity] IS NULL OR [AvailableQuantity] >= 0)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    EXEC(N'ALTER TABLE [Products] ADD CONSTRAINT [CK_Products_MinimumOrderQuantity_Positive] CHECK ([MinimumOrderQuantity] IS NULL OR [MinimumOrderQuantity] > 0)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    EXEC(N'ALTER TABLE [Products] ADD CONSTRAINT [CK_Products_ReservedQuantity_NonNegative] CHECK ([ReservedQuantity] >= 0)');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE INDEX [IX_CatalogProducts_Category_Name] ON [CatalogProducts] ([Category], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE INDEX [IX_CatalogProducts_DefaultUnitId] ON [CatalogProducts] ([DefaultUnitId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnitsOfMeasure_Code] ON [UnitsOfMeasure] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_CatalogProducts_CatalogProductId] FOREIGN KEY ([CatalogProductId]) REFERENCES [CatalogProducts] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    ALTER TABLE [Products] ADD CONSTRAINT [FK_Products_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [UnitsOfMeasure] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014145550_Phase2CompatibilityFoundation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014145550_Phase2CompatibilityFoundation', N'8.0.0');
END;
GO

COMMIT;
GO

