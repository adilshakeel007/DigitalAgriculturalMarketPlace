BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD [ReviewNotes] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD [ReviewedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD [ReviewedByUserId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'Pending';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD [SubmittedAt] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD [SubmittedByUserId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    CREATE INDEX [IX_CatalogProducts_ReviewedByUserId] ON [CatalogProducts] ([ReviewedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_CatalogProducts_SubmittedByUserId_Name_Category_DefaultUnitId_Status] ON [CatalogProducts] ([SubmittedByUserId], [Name], [Category], [DefaultUnitId], [Status]) WHERE [SubmittedByUserId] IS NOT NULL AND [DefaultUnitId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    EXEC(N'ALTER TABLE [CatalogProducts] ADD CONSTRAINT [CK_CatalogProducts_Status] CHECK ([Status] IN (''Pending'', ''Approved'', ''Rejected''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD CONSTRAINT [FK_CatalogProducts_Users_ReviewedByUserId] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    ALTER TABLE [CatalogProducts] ADD CONSTRAINT [FK_CatalogProducts_Users_SubmittedByUserId] FOREIGN KEY ([SubmittedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261014152803_Phase3UnifiedAccountsAndCatalogGovernance', N'8.0.0');
END;
GO

COMMIT;
GO

