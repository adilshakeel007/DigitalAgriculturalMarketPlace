using System.Data;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketplace.Services;

public class OrderWorkflowService
{
    private readonly ApplicationDbContext _db;
    private readonly InventoryService _inventory;
    public OrderWorkflowService(ApplicationDbContext db, InventoryService inventory) { _db = db; _inventory = inventory; }

    public async Task<WorkflowResult> PlaceAsync(int buyerId, int productId, decimal quantity, string operationKey)
    {
        if (quantity <= 0) return WorkflowResult.Fail("Quantity must be greater than zero.");
        await using var tx = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var prior = await _db.InventoryReservations.Include(r => r.OrderItem).FirstOrDefaultAsync(r => r.OperationKey == $"reserve:{operationKey}");
        if (prior?.OrderItem is not null) return WorkflowResult.Ok("This order was already submitted.", prior.OrderItem.OrderId);
        var product = await _db.Products.Include(p => p.UnitOfMeasure).FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null || !product.IsActive) return WorkflowResult.Fail("This listing is no longer available.");
        if (product.FarmerId == buyerId) return WorkflowResult.Fail("You cannot purchase your own listing.");
        var available = product.AvailableQuantity ?? product.Quantity;
        if (quantity < (product.MinimumOrderQuantity ?? 0) || quantity > available) return WorkflowResult.Fail($"Only {available:0.###} {product.UnitOfMeasure?.Code ?? "units"} are available.");
        if (!QuantityRules.IsValid(quantity, product.UnitOfMeasure?.DecimalPlaces ?? 0))
            return WorkflowResult.Fail(QuantityRules.Message(product.UnitOfMeasure?.Code, product.UnitOfMeasure?.DecimalPlaces ?? 0));

        var now = DateTime.UtcNow;
        var order = new Order { ConsumerId=buyerId, ProductId=product.Id, SellerId=product.FarmerId, Quantity=quantity, TotalPrice=decimal.Round(product.PricePerUnit*quantity,2), Subtotal=decimal.Round(product.PricePerUnit*quantity,2), Status="Pending", OrderDate=now, UpdatedAt=now };
        _db.Orders.Add(order); await _db.SaveChangesAsync();
        order.OrderNumber = $"AG-{now:yyyyMMdd}-{order.Id:D6}";
        var item = new OrderItem { OrderId=order.Id, ProductId=product.Id, ProductNameSnapshot=product.Name, UnitNameSnapshot=product.UnitOfMeasure?.Name, Quantity=quantity, UnitPrice=product.PricePerUnit, LineTotal=order.TotalPrice, SellerId=product.FarmerId };
        _db.OrderItems.Add(item); await _db.SaveChangesAsync();
        product.AvailableQuantity=available-quantity; product.ReservedQuantity+=quantity; InventoryService.SyncLegacyQuantity(product);
        _db.InventoryReservations.Add(new InventoryReservation { ProductId=product.Id, OrderItemId=item.Id, Quantity=quantity, Status="Active", ReservedAt=now, OperationKey=$"reserve:{operationKey}" });
        _inventory.AddMovement(product,"ReservationCreated",-quantity,quantity,buyerId,"Order",order.Id,null,$"movement:reserve:{operationKey}");
        _db.OrderStatusHistory.Add(new OrderStatusHistory { OrderId=order.Id, ToStatus="Pending", ChangedByUserId=buyerId, ChangedAt=now, OperationKey=$"status:create:{operationKey}" });
        AddNotification(product.FarmerId,"New order",$"A new order for {quantity:0.###} {product.UnitOfMeasure?.Code ?? "unit"} of {product.Name} is awaiting your review.","Order",order.Id,$"notify:order-created:{operationKey}");
        try { await _db.SaveChangesAsync(); await tx.CommitAsync(); return WorkflowResult.Ok("Your order has been placed successfully.",order.Id); }
        catch (DbUpdateException) { return WorkflowResult.Fail("The order could not be created. It may already have been processed; refresh before retrying."); }
    }

    public async Task<WorkflowResult> TransitionAsync(int orderId, int actorId, bool sellerAction, string target, string? reason, string operationKey)
    {
        await using var tx=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var order=await _db.Orders.Include(o=>o.Product).Include(o=>o.Items).ThenInclude(i=>i.Reservation).FirstOrDefaultAsync(o=>o.Id==orderId);
        if(order?.Product is null) return WorkflowResult.Fail("Order not found.");
        if(sellerAction ? order.Product.FarmerId!=actorId : order.ConsumerId!=actorId) return WorkflowResult.Fail("You are not authorized to change this order.");
        if(await _db.OrderStatusHistory.AnyAsync(h=>h.OperationKey==operationKey)) return WorkflowResult.Ok("This action was already processed.",order.Id);
        var from=order.Status; var valid=(from,target) switch { ("Pending","Confirmed")=>sellerAction, ("Confirmed","Completed")=>sellerAction, ("Pending","Cancelled")=>true, ("Confirmed","Cancelled")=>sellerAction, _=>false };
        if(!valid) return WorkflowResult.Fail("Invalid order status change.");
        var reservation=order.Items.Select(i=>i.Reservation).SingleOrDefault(r=>r is not null);
        if(reservation is null && from is "Pending" or "Confirmed")
        {
            var item=order.Items.SingleOrDefault();
            if(item is null)
            {
                item=new OrderItem { OrderId=order.Id,ProductId=order.ProductId,ProductNameSnapshot=order.Product.Name,Quantity=order.Quantity,UnitPrice=order.Quantity==0?order.TotalPrice:decimal.Round(order.TotalPrice/order.Quantity,2),LineTotal=order.TotalPrice,SellerId=order.Product.FarmerId };
                _db.OrderItems.Add(item); await _db.SaveChangesAsync();
            }
            order.Product.AvailableQuantity ??= order.Product.Quantity;
            order.Product.ReservedQuantity += order.Quantity;
            reservation=new InventoryReservation { ProductId=order.ProductId,OrderItemId=item.Id,Quantity=order.Quantity,Status="Active",ReservedAt=order.OrderDate,OperationKey=$"compat:reservation:{order.Id}" };
            _db.InventoryReservations.Add(reservation);
            _inventory.AddMovement(order.Product,"InventoryCorrection",0,order.Quantity,actorId,"Order",order.Id,"Compatibility reservation reconstructed before transition",$"compat:movement:{order.Id}");
        }
        if(reservation is null || reservation.Status!="Active") return WorkflowResult.Fail("The active inventory reservation is missing or already finalized.");
        var q=reservation.Quantity; var now=DateTime.UtcNow;
        if(target=="Cancelled") { order.Product.ReservedQuantity-=q; order.Product.AvailableQuantity=(order.Product.AvailableQuantity??order.Product.Quantity)+q; reservation.Status="Released"; reservation.ReleasedAt=now; order.CancelledAt=now; order.CancellationReason=reason; _inventory.AddMovement(order.Product,"ReservationReleased",q,-q,actorId,"Order",order.Id,reason,$"movement:{operationKey}"); InventoryService.SyncLegacyQuantity(order.Product); }
        else if(target=="Completed") { order.Product.ReservedQuantity-=q; reservation.Status="Consumed"; reservation.ConsumedAt=now; order.CompletedAt=now; _inventory.AddMovement(order.Product,"OrderFulfilled",0,-q,actorId,"Order",order.Id,reason,$"movement:{operationKey}"); }
        order.Status=target; order.UpdatedAt=now;
        _db.OrderStatusHistory.Add(new OrderStatusHistory { OrderId=order.Id,FromStatus=from,ToStatus=target,ChangedByUserId=actorId,Reason=reason,ChangedAt=now,OperationKey=operationKey });
        var recipient = sellerAction ? order.ConsumerId : order.Product.FarmerId;
        AddNotification(recipient,$"Order {target}",$"Order {order.OrderNumber ?? $"#{order.Id}"} is now {target.ToLowerInvariant()}.","Order",order.Id,$"notify:order-transition:{operationKey}");
        try { await _db.SaveChangesAsync(); await tx.CommitAsync(); return WorkflowResult.Ok("Order status updated successfully.",order.Id); } catch(DbUpdateConcurrencyException) { return WorkflowResult.Fail("The order changed concurrently. Refresh and try again."); } catch(DbUpdateException) { return WorkflowResult.Fail("The order could not be updated. No changes were applied; refresh and try again."); }
    }

    private void AddNotification(int userId,string title,string message,string type,int relatedId,string operationKey)
    {
        if (_db.Notifications.Local.Any(n=>n.OperationKey==operationKey)) return;
        _db.Notifications.Add(new Notification{UserId=userId,Title=title,Message=message,Type=type,RelatedId=relatedId,OperationKey=operationKey,CreatedAt=DateTime.UtcNow});
    }
}
