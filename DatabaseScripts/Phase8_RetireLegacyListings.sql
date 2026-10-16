SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

UPDATE dbo.Products
SET IsActive = 0,
    UpdatedAt = COALESCE(UpdatedAt, SYSUTCDATETIME())
WHERE Name IN (N'Fresh Tomatoes', N'bhindi', N'bhindiyan', N'Apple')
  AND IsActive = 1;

IF (SELECT COUNT(*)
    FROM dbo.Products
    WHERE Name IN (N'Fresh Tomatoes', N'bhindi', N'bhindiyan', N'Apple')) <> 4
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51000, 'Expected exactly four legacy listings; retirement was not applied.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM dbo.Products
    WHERE Name IN (N'Fresh Tomatoes', N'bhindi', N'bhindiyan', N'Apple')
      AND IsActive = 1)
BEGIN
    ROLLBACK TRANSACTION;
    THROW 51001, 'One or more legacy listings remain active.', 1;
END;

COMMIT TRANSACTION;

SELECT Id, Name, IsActive, CatalogProductId, UnitOfMeasureId
FROM dbo.Products
WHERE Name IN (N'Fresh Tomatoes', N'bhindi', N'bhindiyan', N'Apple')
ORDER BY Id;
