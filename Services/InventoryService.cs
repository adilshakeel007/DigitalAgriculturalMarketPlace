using System.Data;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketplace.Services;

public class InventoryService
{
    private readonly ApplicationDbContext _db;
    public InventoryService(ApplicationDbContext db) => _db = db;

    public async Task<WorkflowResult> AdjustAsync(int productId, int sellerId, decimal newAvailable, string reason, string operationKey)
    {
        if (newAvailable < 0) return WorkflowResult.Fail("Available quantity cannot be negative.");
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.FarmerId == sellerId);
        if (product is null) return WorkflowResult.Fail("Listing was not found or does not belong to you.");
        if (product.UnitOfMeasureId is null) return WorkflowResult.Fail("Map a unit of measure before adjusting this legacy listing.");
        var unit = await _db.UnitsOfMeasure.FindAsync(product.UnitOfMeasureId.Value);
        if (unit is null || !QuantityRules.IsValid(newAvailable, unit.DecimalPlaces)) return WorkflowResult.Fail(QuantityRules.Message(unit?.Code, unit?.DecimalPlaces ?? 0));
        if (await _db.InventoryMovements.AnyAsync(x => x.OperationKey == operationKey)) return WorkflowResult.Ok("This adjustment was already applied.");
        var before = product.AvailableQuantity ?? product.Quantity;
        product.AvailableQuantity = newAvailable;
        SyncLegacyQuantity(product);
        AddMovement(product, "ManualAdjustment", newAvailable - before, 0, sellerId, "Product", product.Id, reason, operationKey);
        try { await _db.SaveChangesAsync(); await tx.CommitAsync(); return WorkflowResult.Ok("Inventory adjusted successfully."); }
        catch (DbUpdateConcurrencyException) { return WorkflowResult.Fail("Inventory changed while you were editing. Refresh and try again."); }
        catch (DbUpdateException) { return WorkflowResult.Fail("Inventory could not be updated. No changes were applied; refresh and try again."); }
    }

    internal void AddMovement(Product product, string type, decimal availableDelta, decimal reservedDelta, int? actorId, string referenceType, int referenceId, string? reason, string operationKey)
    {
        _db.InventoryMovements.Add(new InventoryMovement { ProductId = product.Id, MovementType = type, AvailableDelta = availableDelta, ReservedDelta = reservedDelta, AvailableAfter = product.AvailableQuantity ?? product.Quantity, ReservedAfter = product.ReservedQuantity, PerformedByUserId = actorId, ReferenceType = referenceType, ReferenceId = referenceId, Reason = reason, OperationKey = operationKey });
    }

    internal static void SyncLegacyQuantity(Product product)
    {
        var available = product.AvailableQuantity ?? product.Quantity;
        // Quantity is retained for legacy compatibility; AvailableQuantity is authoritative.
        // Mirror only values the old integer column can represent without rounding or truncation.
        if (available == decimal.Truncate(available) && available <= int.MaxValue)
            product.Quantity = decimal.ToInt32(available);
    }
}
