using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Controllers;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace DigitalAgriculturalMarketPlace.Tests;

public sealed class ExistingWorkflowTests : IClassFixture<SqlServerTestDatabase>
{
    private readonly SqlServerTestDatabase _database;

    public ExistingWorkflowTests(SqlServerTestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task Login_ValidActiveAccount_CreatesSession()
    {
        await using var db = _database.CreateContext();
        var user = await AddUser(db, UserRoles.Consumer);
        var controller = WithSession(new AccountController(db));

        var result = await controller.Login(new LoginViewModel
        {
            Email = user.Email,
            Password = "Test123!"
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(user.Id, controller.HttpContext.Session.GetInt32("UserId"));
        Assert.Equal(UserRoles.Consumer, controller.HttpContext.Session.GetString("UserType"));
    }

    [Fact]
    public async Task Login_InactiveAccount_IsRejected()
    {
        await using var db = _database.CreateContext();
        var user = await AddUser(db, UserRoles.Consumer, isActive: false);
        var controller = WithSession(new AccountController(db));

        var result = await controller.Login(new LoginViewModel
        {
            Email = user.Email,
            Password = "Test123!"
        });

        Assert.IsType<ViewResult>(result);
        Assert.Null(controller.HttpContext.Session.GetInt32("UserId"));
    }

    [Fact]
    public void SessionAuthorization_UnauthenticatedUser_IsRedirectedToLogin()
    {
        var context = CreateAuthorizationContext(new TestSession());

        new SessionAuthorizeAttribute().OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public async Task SessionAuthorization_WrongRole_IsRedirectedToAccessDenied()
    {
        await using var db = _database.CreateContext();
        var user = await AddUser(db, UserRoles.Consumer);
        var session = new TestSession();
        session.SetInt32("UserId", user.Id);
        session.SetString("UserType", UserRoles.Consumer);
        var context = CreateAuthorizationContext(session, db);

        new SessionAuthorizeAttribute(UserRoles.Admin)
            .OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("AccessDenied", redirect.ActionName);
        Assert.Equal("Account", redirect.ControllerName);
    }

    [Fact]
    public async Task ProductDetails_EnforcesFarmerOwnership()
    {
        await using var db = _database.CreateContext();
        var owner = await AddUser(db, UserRoles.Farmer);
        var other = await AddUser(db, UserRoles.Farmer);
        var product = await AddProduct(db, owner.Id, 10);
        var environment = new TestHostEnvironment();

        var ownerController = WithSession(new ProductsController(db, environment), owner);
        var otherController = WithSession(new ProductsController(db, environment), other);

        Assert.IsType<ViewResult>(await ownerController.Details(product.Id));
        Assert.IsType<NotFoundResult>(await otherController.Details(product.Id));
    }

    [Fact]
    public async Task PlaceOrder_ValidQuantity_CreatesPendingOrderAndReducesStock()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Farmer);
        var buyer = await AddUser(db, UserRoles.Consumer);
        var product = await AddProduct(db, seller.Id, 10);
        var controller = WithSession(new OrdersController(db), buyer);

        var result = await controller.PlaceOrder(new PlaceOrderViewModel
        {
            ProductId = product.Id,
            Quantity = 3
        });

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(7, (await db.Products.FindAsync(product.Id))!.Quantity);
        Assert.True(await db.Orders.AnyAsync(o =>
            o.ConsumerId == buyer.Id && o.ProductId == product.Id &&
            o.Quantity == 3 && o.Status == "Pending"));
    }

    [Fact]
    public async Task PlaceOrder_ZeroQuantity_IsRejectedWithoutChangingStock()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Farmer);
        var buyer = await AddUser(db, UserRoles.Consumer);
        var product = await AddProduct(db, seller.Id, 10);
        var controller = WithSession(new OrdersController(db), buyer);

        var result = await controller.PlaceOrder(new PlaceOrderViewModel
        {
            ProductId = product.Id,
            Quantity = 0
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(10, (await db.Products.FindAsync(product.Id))!.Quantity);
        Assert.False(await db.Orders.AnyAsync(o => o.ProductId == product.Id));
    }

    [Fact]
    public async Task PlaceOrder_InsufficientInventory_IsRejected()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Farmer);
        var buyer = await AddUser(db, UserRoles.Consumer);
        var product = await AddProduct(db, seller.Id, 2);
        var controller = WithSession(new OrdersController(db), buyer);

        var result = await controller.PlaceOrder(new PlaceOrderViewModel
        {
            ProductId = product.Id,
            Quantity = 3
        });

        Assert.IsType<ViewResult>(result);
        Assert.Equal(2, (await db.Products.FindAsync(product.Id))!.Quantity);
        Assert.False(await db.Orders.AnyAsync(o => o.ProductId == product.Id));
    }

    [Fact]
    public async Task Seller_CanConfirmPendingOrder()
    {
        await using var db = _database.CreateContext();
        var (seller, _, _, order) = await AddOrder(db, "Pending", 7, 3);
        var controller = WithSession(new OrdersController(db), seller);

        await controller.UpdateStatus(order.Id, "Confirmed");

        Assert.Equal("Confirmed", (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Buyer_CancelPendingOrder_RestoresStockOnlyOnce()
    {
        await using var db = _database.CreateContext();
        var (_, buyer, product, order) = await AddOrder(db, "Pending", 7, 3);
        var controller = WithSession(new OrdersController(db), buyer);

        await controller.ConsumerCancelOrder(order.Id);
        int afterFirstCancellation = (await db.Products.FindAsync(product.Id))!.Quantity;
        await controller.ConsumerCancelOrder(order.Id);

        Assert.Equal(10, afterFirstCancellation);
        Assert.Equal(10, (await db.Products.FindAsync(product.Id))!.Quantity);
        Assert.Equal("Cancelled", (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Seller_CancelPendingOrder_RestoresStock()
    {
        await using var db = _database.CreateContext();
        var (seller, _, product, order) = await AddOrder(db, "Pending", 7, 3);
        var controller = WithSession(new OrdersController(db), seller);

        await controller.UpdateStatus(order.Id, "Cancelled");

        Assert.Equal(10, (await db.Products.FindAsync(product.Id))!.Quantity);
        Assert.Equal("Cancelled", (await db.Orders.FindAsync(order.Id))!.Status);
    }

    [Fact]
    public async Task Seller_CannotTransitionCompletedOrder()
    {
        await using var db = _database.CreateContext();
        var (seller, _, product, order) = await AddOrder(db, "Completed", 7, 3);
        var controller = WithSession(new OrdersController(db), seller);

        await controller.UpdateStatus(order.Id, "Cancelled");

        Assert.Equal("Completed", (await db.Orders.FindAsync(order.Id))!.Status);
        Assert.Equal(7, (await db.Products.FindAsync(product.Id))!.Quantity);
    }

    [Fact]
    public async Task ReviewEligibility_RequiresCompletedOrder()
    {
        await using var db = _database.CreateContext();
        var (_, buyerWithCompletedOrder, product, _) =
            await AddOrder(db, "Completed", 7, 3);
        var buyerWithoutCompletedOrder = await AddUser(db, UserRoles.Consumer);

        var eligible = WithSession(new ReviewsController(db), buyerWithCompletedOrder);
        var ineligible = WithSession(new ReviewsController(db), buyerWithoutCompletedOrder);

        Assert.IsType<ViewResult>(await eligible.Create(product.Id));
        Assert.IsType<RedirectToActionResult>(await ineligible.Create(product.Id));
    }

    private static T WithSession<T>(T controller, User? user = null) where T : Controller
    {
        var httpContext = new DefaultHttpContext
        {
            Session = new TestSession()
        };

        if (user != null)
        {
            httpContext.Session.SetInt32("UserId", user.Id);
            httpContext.Session.SetString("UserName", user.Name);
            httpContext.Session.SetString("UserType", user.UserType);
        }

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        controller.TempData = new TempDataDictionary(
            httpContext,
            new TestTempDataProvider());

        return controller;
    }

    private static ActionExecutingContext CreateAuthorizationContext(
        ISession session,
        DigitalAgriculturalMarketPlace.Data.ApplicationDbContext? db = null)
    {
        var httpContext = new DefaultHttpContext { Session = session };
        if (db != null)
        {
            httpContext.RequestServices = new ServiceCollection()
                .AddSingleton(db)
                .BuildServiceProvider();
        }
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }

    private static async Task<User> AddUser(
        DigitalAgriculturalMarketPlace.Data.ApplicationDbContext db,
        string role,
        bool isActive = true)
    {
        string key = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Name = $"Test {role}",
            Email = $"{key}@example.test",
            Phone = "0000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            UserType = role,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<Product> AddProduct(
        DigitalAgriculturalMarketPlace.Data.ApplicationDbContext db,
        int sellerId,
        int quantity)
    {
        var product = new Product
        {
            Name = $"Test product {Guid.NewGuid():N}",
            Category = "Test",
            PricePerUnit = 25m,
            Quantity = quantity,
            FarmerId = sellerId,
            IsActive = true,
            ListedAt = DateTime.UtcNow
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    private static async Task<(User Seller, User Buyer, Product Product, Order Order)> AddOrder(
        DigitalAgriculturalMarketPlace.Data.ApplicationDbContext db,
        string status,
        int remainingStock,
        int orderQuantity)
    {
        var seller = await AddUser(db, UserRoles.Farmer);
        var buyer = await AddUser(db, UserRoles.Consumer);
        var product = await AddProduct(db, seller.Id, remainingStock);
        var order = new Order
        {
            ConsumerId = buyer.Id,
            ProductId = product.Id,
            Quantity = orderQuantity,
            TotalPrice = product.PricePerUnit * orderQuantity,
            Status = status,
            OrderDate = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return (seller, buyer, product, order);
    }
}

internal sealed class TestHostEnvironment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "DigitalAgriculturalMarketPlace.Tests";
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = Path.GetTempPath();
    public string EnvironmentName { get; set; } = "Test";
    public string ContentRootPath { get; set; } = Path.GetTempPath();
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}

internal sealed class TestTempDataProvider : ITempDataProvider
{
    private IDictionary<string, object> _values =
        new Dictionary<string, object>();

    public IDictionary<string, object> LoadTempData(HttpContext context)
        => new Dictionary<string, object>(_values);

    public void SaveTempData(
        HttpContext context,
        IDictionary<string, object> values)
    {
        _values = new Dictionary<string, object>(values);
    }
}
