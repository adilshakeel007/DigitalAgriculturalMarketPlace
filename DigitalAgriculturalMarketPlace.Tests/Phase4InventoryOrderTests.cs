using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DigitalAgriculturalMarketPlace.Tests;

public sealed class Phase4InventoryOrderTests : IClassFixture<SqlServerTestDatabase>
{
    private readonly SqlServerTestDatabase _database;
    public Phase4InventoryOrderTests(SqlServerTestDatabase database)=>_database=database;

    [Fact] public async Task Placement_CreatesReservationMovementItemAndHistoryAtomically()
    {
        await using var db=_database.CreateContext(); var (seller,buyer,product)=await Seed(db,10);
        var result=await Workflow(db).PlaceAsync(buyer.Id,product.Id,3,"phase4-place-"+Guid.NewGuid());
        Assert.True(result.Succeeded); await db.Entry(product).ReloadAsync();
        Assert.Equal(7,product.AvailableQuantity); Assert.Equal(3,product.ReservedQuantity); Assert.Equal(7,product.Quantity);
        Assert.True(await db.InventoryReservations.AnyAsync(r=>r.OrderItem!.OrderId==result.OrderId&&r.Status=="Active"));
        Assert.True(await db.InventoryMovements.AnyAsync(m=>m.ReferenceId==result.OrderId&&m.MovementType=="ReservationCreated"));
        Assert.True(await db.OrderStatusHistory.AnyAsync(h=>h.OrderId==result.OrderId&&h.ToStatus=="Pending"));
        Assert.Equal(product.PricePerUnit*3,(await db.OrderItems.SingleAsync(i=>i.OrderId==result.OrderId)).LineTotal);
    }

    [Fact] public async Task InsufficientStock_RollsBackEntireWorkflow()
    {
        await using var db=_database.CreateContext(); var (_,buyer,p)=await Seed(db,2); var key=Guid.NewGuid().ToString();
        Assert.False((await Workflow(db).PlaceAsync(buyer.Id,p.Id,3,key)).Succeeded);
        Assert.False(await db.Orders.AnyAsync(o=>o.ProductId==p.Id)); Assert.False(await db.InventoryMovements.AnyAsync(m=>m.ProductId==p.Id));
    }

    [Fact] public async Task DuplicateSubmission_CreatesOneOrder()
    {
        await using var db=_database.CreateContext(); var (_,buyer,p)=await Seed(db,5); var key="duplicate-"+Guid.NewGuid(); var workflow=Workflow(db);
        var first=await workflow.PlaceAsync(buyer.Id,p.Id,2,key); var second=await workflow.PlaceAsync(buyer.Id,p.Id,2,key);
        Assert.True(first.Succeeded); Assert.True(second.Succeeded); Assert.Equal(first.OrderId,second.OrderId); Assert.Equal(1,await db.Orders.CountAsync(o=>o.ProductId==p.Id));
    }

    [Fact] public async Task Confirmation_DoesNotDeductStockAgain()
    {
        await using var db=_database.CreateContext(); var (seller,buyer,p)=await Seed(db,6); var w=Workflow(db); var placed=await w.PlaceAsync(buyer.Id,p.Id,2,Guid.NewGuid().ToString());
        var before=(await db.Products.FindAsync(p.Id))!.AvailableQuantity; Assert.True((await w.TransitionAsync(placed.OrderId!.Value,seller.Id,true,"Confirmed",null,"confirm-"+placed.OrderId)).Succeeded);
        Assert.Equal(before,(await db.Products.FindAsync(p.Id))!.AvailableQuantity);
    }

    [Fact] public async Task Completion_ConsumesReservationOnlyOnce()
    {
        await using var db=_database.CreateContext(); var (seller,buyer,p)=await Seed(db,8); var w=Workflow(db); var placed=await w.PlaceAsync(buyer.Id,p.Id,3,Guid.NewGuid().ToString());
        await w.TransitionAsync(placed.OrderId!.Value,seller.Id,true,"Confirmed",null,"c-"+placed.OrderId); var first=await w.TransitionAsync(placed.OrderId.Value,seller.Id,true,"Completed",null,"done-"+placed.OrderId); var repeat=await w.TransitionAsync(placed.OrderId.Value,seller.Id,true,"Completed",null,"done2-"+placed.OrderId);
        Assert.True(first.Succeeded); Assert.False(repeat.Succeeded); Assert.Equal(0,(await db.Products.FindAsync(p.Id))!.ReservedQuantity); Assert.Equal("Consumed",(await db.InventoryReservations.SingleAsync(r=>r.OrderItem!.OrderId==placed.OrderId)).Status);
    }

    [Fact] public async Task PendingCancellation_ReleasesReservationOnlyOnce()
    {
        await using var db=_database.CreateContext(); var (_,buyer,p)=await Seed(db,8); var w=Workflow(db); var placed=await w.PlaceAsync(buyer.Id,p.Id,3,Guid.NewGuid().ToString());
        Assert.True((await w.TransitionAsync(placed.OrderId!.Value,buyer.Id,false,"Cancelled","Changed mind","cancel-"+placed.OrderId)).Succeeded); Assert.True((await w.TransitionAsync(placed.OrderId.Value,buyer.Id,false,"Cancelled",null,"cancel-"+placed.OrderId)).Succeeded);
        var saved=await db.Products.FindAsync(p.Id); Assert.Equal(8,saved!.AvailableQuantity); Assert.Equal(0,saved.ReservedQuantity); Assert.Equal(1,await db.InventoryMovements.CountAsync(m=>m.MovementType=="ReservationReleased"&&m.ReferenceId==placed.OrderId));
    }

    [Fact] public async Task ConfirmedOrder_SellerCanCancelAndRelease()
    {
        await using var db=_database.CreateContext(); var (seller,buyer,p)=await Seed(db,4); var w=Workflow(db); var placed=await w.PlaceAsync(buyer.Id,p.Id,2,Guid.NewGuid().ToString()); await w.TransitionAsync(placed.OrderId!.Value,seller.Id,true,"Confirmed",null,"cc-"+placed.OrderId);
        Assert.True((await w.TransitionAsync(placed.OrderId.Value,seller.Id,true,"Cancelled","Cannot fulfil","sc-"+placed.OrderId)).Succeeded); Assert.Equal(4,(await db.Products.FindAsync(p.Id))!.AvailableQuantity);
    }

    [Fact] public async Task PriceSnapshot_DoesNotChangeWhenListingPriceChanges()
    {
        await using var db=_database.CreateContext(); var (_,buyer,p)=await Seed(db,4); var placed=await Workflow(db).PlaceAsync(buyer.Id,p.Id,2,Guid.NewGuid().ToString()); var item=await db.OrderItems.SingleAsync(i=>i.OrderId==placed.OrderId); p.PricePerUnit=999; await db.SaveChangesAsync(); Assert.Equal(25,item.UnitPrice); Assert.Equal(50,item.LineTotal);
    }

    [Fact] public async Task InventoryAdjustment_RequiresOwnershipAndMappedUnit()
    {
        await using var db=_database.CreateContext(); var (seller,buyer,p)=await Seed(db,4); var service=new InventoryService(db);
        Assert.False((await service.AdjustAsync(p.Id,buyer.Id,5,"unauthorized","x-"+Guid.NewGuid())).Succeeded); p.UnitOfMeasureId=null; await db.SaveChangesAsync(); Assert.False((await service.AdjustAsync(p.Id,seller.Id,5,"unknown unit","x-"+Guid.NewGuid())).Succeeded);
    }

    [Fact] public async Task Adjustment_IsLedgerBackedAndIdempotent()
    {
        await using var db=_database.CreateContext(); var (seller,_,p)=await Seed(db,4); var service=new InventoryService(db); var key="adjust-"+Guid.NewGuid(); Assert.True((await service.AdjustAsync(p.Id,seller.Id,9,"Counted stock",key)).Succeeded); Assert.True((await service.AdjustAsync(p.Id,seller.Id,9,"Counted stock",key)).Succeeded); Assert.Equal(1,await db.InventoryMovements.CountAsync(m=>m.OperationKey==key)); Assert.Equal(9,(await db.Products.FindAsync(p.Id))!.Quantity);
    }

    [Fact] public async Task FractionalOrder_ReservesExactQuantityWithoutRounding()
    {
        await using var db=_database.CreateContext(); var (_,buyer,p)=await Seed(db,4); var result=await Workflow(db).PlaceAsync(buyer.Id,p.Id,1.5m,Guid.NewGuid().ToString()); Assert.True(result.Succeeded); var order=await db.Orders.FindAsync(result.OrderId); Assert.Equal(1.5m,order!.Quantity); Assert.Equal(2.5m,(await db.Products.FindAsync(p.Id))!.AvailableQuantity); Assert.Equal(1.5m,await db.InventoryReservations.Where(r=>r.OrderItem!.OrderId==order.Id).Select(r=>r.Quantity).SingleAsync());
    }

    [Fact] public async Task DifferentSeller_CannotTransitionOrder()
    {
        await using var db=_database.CreateContext(); var (_,buyer,p)=await Seed(db,4); var other=await AddUser(db,UserRoles.Farmer); var w=Workflow(db); var placed=await w.PlaceAsync(buyer.Id,p.Id,1,Guid.NewGuid().ToString()); Assert.False((await w.TransitionAsync(placed.OrderId!.Value,other.Id,true,"Confirmed",null,Guid.NewGuid().ToString())).Succeeded);
    }

    [Fact] public async Task WholeOnlyUnit_RejectsFractionalOrder()
    {
        await using var db=_database.CreateContext();var(_,buyer,p)=await Seed(db,4);p.UnitOfMeasureId=3;await db.SaveChangesAsync();var result=await Workflow(db).PlaceAsync(buyer.Id,p.Id,1.5m,Guid.NewGuid().ToString());Assert.False(result.Succeeded);Assert.Contains("whole",result.Message,StringComparison.OrdinalIgnoreCase);
    }

    [Fact] public async Task QuantityBeyondUnitPrecision_IsRejected()
    {
        await using var db=_database.CreateContext();var(_,buyer,p)=await Seed(db,4);var result=await Workflow(db).PlaceAsync(buyer.Id,p.Id,1.2345m,Guid.NewGuid().ToString());Assert.False(result.Succeeded);Assert.Contains("3 decimal",result.Message);
    }

    [Fact] public async Task FractionalCancellation_RestoresExactAvailableQuantityOnce()
    {
        await using var db=_database.CreateContext();var(_,buyer,p)=await Seed(db,4);var w=Workflow(db);var placed=await w.PlaceAsync(buyer.Id,p.Id,1.125m,Guid.NewGuid().ToString());var key="fractional-cancel-"+Guid.NewGuid();Assert.True((await w.TransitionAsync(placed.OrderId!.Value,buyer.Id,false,"Cancelled","Changed mind",key)).Succeeded);Assert.True((await w.TransitionAsync(placed.OrderId.Value,buyer.Id,false,"Cancelled","Changed mind",key)).Succeeded);var saved=await db.Products.FindAsync(p.Id);Assert.Equal(4m,saved!.AvailableQuantity);Assert.Equal(0m,saved.ReservedQuantity);
    }

    [Fact] public async Task DuplicateOrderSubmission_CreatesOneSellerNotification()
    {
        await using var db=_database.CreateContext();var(seller,buyer,p)=await Seed(db,5);var w=Workflow(db);var key="notify-"+Guid.NewGuid();var first=await w.PlaceAsync(buyer.Id,p.Id,1.25m,key);var repeat=await w.PlaceAsync(buyer.Id,p.Id,1.25m,key);Assert.Equal(first.OrderId,repeat.OrderId);Assert.Equal(1,await db.Notifications.CountAsync(n=>n.UserId==seller.Id&&n.OperationKey=="notify:order-created:"+key));
    }

    [Fact] public async Task FractionalInventoryAdjustment_IsExactAndLedgerBacked()
    {
        await using var db=_database.CreateContext();var(seller,_,p)=await Seed(db,4);var service=new InventoryService(db);var key="fractional-adjust-"+Guid.NewGuid();Assert.True((await service.AdjustAsync(p.Id,seller.Id,3.375m,"Weighed stock",key)).Succeeded);var saved=await db.Products.FindAsync(p.Id);Assert.Equal(3.375m,saved!.AvailableQuantity);Assert.Equal(4,saved.Quantity);Assert.Equal(-0.625m,(await db.InventoryMovements.SingleAsync(m=>m.OperationKey==key)).AvailableDelta);
    }

    [Fact] public async Task ConcurrentPurchases_CannotOversellOrDoubleReserve()
    {
        int buyer1Id,buyer2Id,productId;
        await using(var seed=_database.CreateContext()){var(_,buyer1,p)=await Seed(seed,10);var buyer2=await AddUser(seed,UserRoles.Consumer);buyer1Id=buyer1.Id;buyer2Id=buyer2.Id;productId=p.Id;}
        async Task<bool> Place(int buyerId){try{await using var db=_database.CreateContext();return(await Workflow(db).PlaceAsync(buyerId,productId,6,"concurrent-order-"+Guid.NewGuid())).Succeeded;}catch{return false;}}
        var results=await Task.WhenAll(Task.Run(()=>Place(buyer1Id)),Task.Run(()=>Place(buyer2Id)));
        await using var check=_database.CreateContext();var product=await check.Products.FindAsync(productId);Assert.Equal(1,results.Count(x=>x));Assert.Equal(4m,product!.AvailableQuantity);Assert.Equal(6m,product.ReservedQuantity);Assert.Equal(1,await check.InventoryReservations.CountAsync(r=>r.ProductId==productId&&r.Status=="Active"));
    }

    private static OrderWorkflowService Workflow(ApplicationDbContext db)=>new(db,new InventoryService(db));
    private static async Task<(User Seller,User Buyer,Product Product)> Seed(ApplicationDbContext db,int quantity)
    {
        var seller=await AddUser(db,UserRoles.Farmer); var buyer=await AddUser(db,UserRoles.Consumer); var p=new Product{Name="Phase4 crop "+Guid.NewGuid(),Category="Test",PricePerUnit=25,Quantity=quantity,AvailableQuantity=quantity,ReservedQuantity=0,FarmerId=seller.Id,UnitOfMeasureId=1,IsActive=true,ListedAt=DateTime.UtcNow}; db.Products.Add(p); await db.SaveChangesAsync(); return(seller,buyer,p);
    }
    private static async Task<User> AddUser(ApplicationDbContext db,string role){var u=new User{Name="Phase4 user",Email=Guid.NewGuid()+"@test.local",Phone="0000000000",PasswordHash="test",UserType=role,IsActive=true,CreatedAt=DateTime.UtcNow};db.Users.Add(u);await db.SaveChangesAsync();return u;}
}
