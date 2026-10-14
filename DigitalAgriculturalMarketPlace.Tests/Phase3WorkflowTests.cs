using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Controllers;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DigitalAgriculturalMarketPlace.Tests;

public sealed class Phase3WorkflowTests : IClassFixture<SqlServerTestDatabase>
{
    private readonly SqlServerTestDatabase _database;

    public Phase3WorkflowTests(SqlServerTestDatabase database)
    {
        _database = database;
    }

    [Fact]
    public async Task ExistingFarmer_IsAuthorizedAsMarketplaceMember()
    {
        await using var db = _database.CreateContext();
        var farmer = await AddUser(db, UserRoles.Farmer);
        var session = SessionFor(farmer);
        var context = AuthorizationContext(session, db);

        new SessionAuthorizeAttribute(UserRoles.MarketplaceMember)
            .OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task Admin_IsBlockedFromMarketplaceMemberWorkflow()
    {
        await using var db = _database.CreateContext();
        var admin = await AddUser(db, UserRoles.Admin);
        var context = AuthorizationContext(SessionFor(admin), db);

        new SessionAuthorizeAttribute(UserRoles.MarketplaceMember)
            .OnActionExecuting(context);

        Assert.IsType<RedirectToActionResult>(context.Result);
    }

    [Fact]
    public async Task InactiveSession_IsRejectedAfterDatabaseRevalidation()
    {
        await using var db = _database.CreateContext();
        var user = await AddUser(db, UserRoles.Consumer, false);
        var session = SessionFor(user);
        var context = AuthorizationContext(session, db);

        new SessionAuthorizeAttribute(UserRoles.MarketplaceMember)
            .OnActionExecuting(context);

        var redirect = Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal("Login", redirect.ActionName);
        Assert.Null(session.GetInt32("UserId"));
    }

    [Fact]
    public async Task StaleSessionRole_CannotBypassDatabaseAuthorization()
    {
        await using var db = _database.CreateContext();
        var user = await AddUser(db, UserRoles.Consumer);
        var session = SessionFor(user);
        session.SetString("UserType", UserRoles.Admin);
        var context = AuthorizationContext(session, db);

        new SessionAuthorizeAttribute(UserRoles.Admin)
            .OnActionExecuting(context);

        Assert.IsType<RedirectToActionResult>(context.Result);
        Assert.Equal(UserRoles.Consumer, session.GetString("UserType"));
    }

    [Fact]
    public async Task Consumer_CanActivateSellerProfile_Idempotently()
    {
        await using var db = _database.CreateContext();
        var consumer = await AddUser(db, UserRoles.Consumer);
        var controller = WithSession(new SellerProfilesController(db), consumer);
        var model = new SellerProfileViewModel
        {
            DisplayName = "Consumer Farm",
            Description = "Seasonal produce"
        };

        Assert.IsType<RedirectToActionResult>(await controller.Activate(model));
        Assert.IsType<RedirectToActionResult>(await controller.Activate(model));

        Assert.Equal(1, await db.SellerProfiles.CountAsync(s => s.UserId == consumer.Id));
    }

    [Fact]
    public async Task SellerProfileEdit_OnlyChangesCurrentUsersProfile()
    {
        await using var db = _database.CreateContext();
        var first = await AddUser(db, UserRoles.Consumer);
        var second = await AddUser(db, UserRoles.Consumer);
        var firstProfile = await AddSellerProfile(db, first, "First seller");
        var secondProfile = await AddSellerProfile(db, second, "Second seller");
        var controller = WithSession(new SellerProfilesController(db), first);

        await controller.Edit(new SellerProfileViewModel
        {
            DisplayName = "Updated first seller",
            RowVersion = firstProfile.RowVersion
        });

        Assert.Equal("Updated first seller", (await db.SellerProfiles.FindAsync(first.Id))!.DisplayName);
        Assert.Equal("Second seller", (await db.SellerProfiles.FindAsync(second.Id))!.DisplayName);
    }

    [Fact]
    public async Task Seller_CanSubmitCatalogProposal_OnlyOnce()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Consumer);
        await AddSellerProfile(db, seller, "Seller");
        var controller = WithSession(new CatalogProductsController(db), seller);
        var model = Proposal("Phase3 Millet");

        Assert.IsType<RedirectToActionResult>(await controller.Create(model));
        Assert.IsType<RedirectToActionResult>(await controller.Create(Proposal("Phase3 Millet")));

        Assert.Equal(1, await db.CatalogProducts.CountAsync(c =>
            c.SubmittedByUserId == seller.Id && c.Name == "Phase3 Millet"));
        Assert.Equal("Pending", (await db.CatalogProducts.SingleAsync(c =>
            c.SubmittedByUserId == seller.Id && c.Name == "Phase3 Millet")).Status);
    }

    [Fact]
    public async Task NonAdmin_CannotUseCatalogModerationAuthorization()
    {
        await using var db = _database.CreateContext();
        var user = await AddUser(db, UserRoles.Consumer);
        var context = AuthorizationContext(SessionFor(user), db);

        new SessionAuthorizeAttribute(UserRoles.Admin).OnActionExecuting(context);

        Assert.IsType<RedirectToActionResult>(context.Result);
    }

    [Fact]
    public async Task Admin_CanApproveCatalogProposal()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Consumer);
        var admin = await AddUser(db, UserRoles.Admin);
        var proposal = await AddProposal(db, seller, "Phase3 Approved Crop");
        var controller = WithSession(new CatalogProductsController(db), admin);

        await controller.Moderate(new CatalogModerationViewModel
        {
            Id = proposal.Id,
            RowVersion = proposal.RowVersion,
            ReviewNotes = "Approved for listing"
        }, "Approved");

        var saved = await db.CatalogProducts.FindAsync(proposal.Id);
        Assert.Equal("Approved", saved!.Status);
        Assert.True(saved.IsActive);
        Assert.Equal(admin.Id, saved.ReviewedByUserId);
    }

    [Fact]
    public async Task Admin_CanRejectProposal_AndItIsNotApproved()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Consumer);
        var admin = await AddUser(db, UserRoles.Admin);
        var proposal = await AddProposal(db, seller, "Phase3 Rejected Crop");
        var controller = WithSession(new CatalogProductsController(db), admin);

        await controller.Moderate(new CatalogModerationViewModel
        {
            Id = proposal.Id,
            RowVersion = proposal.RowVersion
        }, "Rejected");

        Assert.False(await db.CatalogProducts.AnyAsync(c =>
            c.Id == proposal.Id && c.Status == "Approved" && c.IsActive));
    }

    [Fact]
    public async Task User_CannotPurchaseOwnListing()
    {
        await using var db = _database.CreateContext();
        var seller = await AddUser(db, UserRoles.Farmer);
        var product = await AddProduct(db, seller.Id);
        var controller = WithSession(new OrdersController(db), seller);

        var result = await controller.PlaceOrder(new PlaceOrderViewModel
        {
            ProductId = product.Id,
            Quantity = 1
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(await db.Orders.AnyAsync(o =>
            o.ConsumerId == seller.Id && o.ProductId == product.Id));
    }

    [Fact]
    public async Task SellerProfileConcurrencyConflict_ReturnsFriendlyValidationError()
    {
        int userId;
        byte[] staleVersion;
        await using (var seed = _database.CreateContext())
        {
            var user = await AddUser(seed, UserRoles.Consumer);
            var profile = await AddSellerProfile(seed, user, "Original");
            userId = user.Id;
            staleVersion = profile.RowVersion.ToArray();
        }

        await using (var first = _database.CreateContext())
        {
            var profile = await first.SellerProfiles.FindAsync(userId);
            profile!.DisplayName = "Changed elsewhere";
            await first.SaveChangesAsync();
        }

        await using var second = _database.CreateContext();
        var currentUser = await second.Users.FindAsync(userId);
        var controller = WithSession(new SellerProfilesController(second), currentUser!);
        var result = await controller.Edit(new SellerProfileViewModel
        {
            DisplayName = "Stale edit",
            RowVersion = staleVersion
        });

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal("Changed elsewhere", (await second.SellerProfiles.FindAsync(userId))!.DisplayName);
    }

    private static CatalogProductProposalViewModel Proposal(string name) => new()
    {
        Name = name,
        Category = "Grains",
        Description = "Test catalog proposal",
        DefaultUnitId = 1
    };

    private static async Task<CatalogProduct> AddProposal(
        ApplicationDbContext db,
        User seller,
        string name)
    {
        var now = DateTime.UtcNow;
        var proposal = new CatalogProduct
        {
            Name = name,
            Category = "Test",
            Description = "Test proposal",
            DefaultUnitId = 1,
            Status = "Pending",
            IsActive = false,
            SubmittedByUserId = seller.Id,
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.CatalogProducts.Add(proposal);
        await db.SaveChangesAsync();
        return proposal;
    }

    private static async Task<SellerProfile> AddSellerProfile(
        ApplicationDbContext db,
        User user,
        string name)
    {
        var profile = new SellerProfile
        {
            UserId = user.Id,
            DisplayName = name,
            IsActive = true,
            VerificationStatus = "Unverified",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.SellerProfiles.Add(profile);
        await db.SaveChangesAsync();
        return profile;
    }

    private static async Task<Product> AddProduct(ApplicationDbContext db, int sellerId)
    {
        var product = new Product
        {
            Name = $"Product {Guid.NewGuid():N}",
            Category = "Test",
            PricePerUnit = 10,
            Quantity = 5,
            FarmerId = sellerId,
            IsActive = true,
            ListedAt = DateTime.UtcNow
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    private static async Task<User> AddUser(
        ApplicationDbContext db,
        string role,
        bool active = true)
    {
        var user = new User
        {
            Name = $"Test {role}",
            Email = $"{Guid.NewGuid():N}@example.test",
            Phone = "0000000000",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
            UserType = role,
            IsActive = active,
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static TestSession SessionFor(User user)
    {
        var session = new TestSession();
        session.SetInt32("UserId", user.Id);
        session.SetString("UserName", user.Name);
        session.SetString("UserType", user.UserType);
        return session;
    }

    private static ActionExecutingContext AuthorizationContext(
        ISession session,
        ApplicationDbContext db)
    {
        var http = new DefaultHttpContext
        {
            Session = session,
            RequestServices = new ServiceCollection()
                .AddSingleton(db)
                .BuildServiceProvider()
        };
        return new ActionExecutingContext(
            new ActionContext(http, new RouteData(), new ActionDescriptor(), new ModelStateDictionary()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }

    private static T WithSession<T>(T controller, User user) where T : Controller
    {
        var http = new DefaultHttpContext { Session = SessionFor(user) };
        controller.ControllerContext = new ControllerContext { HttpContext = http };
        controller.TempData = new TempDataDictionary(http, new TestTempDataProvider());
        return controller;
    }
}
