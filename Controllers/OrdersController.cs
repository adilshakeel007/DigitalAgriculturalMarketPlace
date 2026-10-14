using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using DigitalAgriculturalMarketplace.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalAgriculturalMarketPlace.Controllers
{

    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderWorkflowService _workflow;

        public OrdersController(ApplicationDbContext context) : this(context, new OrderWorkflowService(context, new InventoryService(context))) { }
        [ActivatorUtilitiesConstructor]
        public OrdersController(ApplicationDbContext context, OrderWorkflowService workflow)
        {
            _context = context;
            _workflow = workflow;
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> PlaceOrder(int productId)
        {
            int? buyerId = HttpContext.Session.GetInt32("UserId");
            Product? product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.IsActive &&
                    (p.AvailableQuantity ?? p.Quantity) > 0 &&
                    p.FarmerId != buyerId
                );

            if (product == null)
            {
                return NotFound();
            }

            PlaceOrderViewModel model =
                new PlaceOrderViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    PricePerUnit = product.PricePerUnit,
                    AvailableQuantity = product.AvailableQuantity ?? product.Quantity,
                    Quantity = 1
                };

            return View(model);

        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
    PlaceOrderViewModel model)
        {
            int? consumerId =
                HttpContext.Session.GetInt32("UserId");

            if (consumerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            Product? product =
                await _context.Products
                    .Include(p => p.UnitOfMeasure)
                    .FirstOrDefaultAsync(p =>
                        p.Id == model.ProductId
                    );

            if (product == null)
            {
                return NotFound();
            }

            if (product.FarmerId == consumerId.Value)
            {
                ModelState.AddModelError("", "You cannot purchase your own listing.");
                model.ProductName = product.Name;
                model.PricePerUnit = product.PricePerUnit;
                model.AvailableQuantity = product.Quantity;
                return View(model);
            }

            // Refill display-only values
            model.ProductName = product.Name;
            model.PricePerUnit = product.PricePerUnit;
            model.AvailableQuantity = product.AvailableQuantity ?? product.Quantity;
            model.UnitName = product.UnitOfMeasure?.Name;

            if (!product.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "This product is no longer available."
                );

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _workflow.PlaceAsync(consumerId.Value, product.Id, model.Quantity, model.OperationKey);
            if (!result.Succeeded) { ModelState.AddModelError("", result.Message); return View(model); }
            await CreateNotification(
    product.FarmerId,
    "New Order",
    $"A new order has been placed for {product.Name}.",
    "Order",
    result.OrderId
);

            TempData["SuccessMessage"] =
                "Your order has been placed successfully.";

            return RedirectToAction(
                "OrderSuccess",
                new { id = result.OrderId }
            );
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            int? consumerId =
                HttpContext.Session.GetInt32("UserId");

            Order? order =
                await _context.Orders
                    .Include(o => o.Product)
                    .FirstOrDefaultAsync(o =>
                        o.Id == id &&
                        o.ConsumerId == consumerId
                    );

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> FarmerOrders(string? status)
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            if (farmerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Consumer)
                .Include(o => o.Items)
                .Where(o =>
                    o.Product != null &&
                    o.Product.FarmerId == farmerId.Value
                );

            string[] allowedStatuses = { "Pending", "Confirmed", "Completed", "Cancelled" };
            if (!string.IsNullOrWhiteSpace(status) && allowedStatuses.Contains(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            ViewBag.Status = status;

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> FarmerOrderDetails(int id)
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            if (farmerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            Order? order = await _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Consumer)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.Product != null &&
                    o.Product.FarmerId == farmerId.Value
                );

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
     int id,
     string status,
     string? reason = null)
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            if (farmerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var operationKey=$"seller:{id}:{status.ToLowerInvariant()}";
            var result=await _workflow.TransitionAsync(id,farmerId.Value,true,status,string.IsNullOrWhiteSpace(reason)?null:reason.Trim(),operationKey);
            if(!result.Succeeded) { TempData["ErrorMessage"]=result.Message; return RedirectToAction("FarmerOrderDetails",new{id}); }
            if (status == "Confirmed")
            {
                await CreateNotification(
                    (await _context.Orders.FindAsync(id))!.ConsumerId,
                    "Order Confirmed",
                    $"Your order #{id} has been confirmed.",
                    "Order",
                    id
                );
            }
            if (status == "Completed")
            {
                await CreateNotification(
                    (await _context.Orders.FindAsync(id))!.ConsumerId,
                    "Order Completed",
                    $"Your order #{id} has been completed.",
                    "Order",
                    id
                );
            }
            if (status == "Cancelled")
            {
                await CreateNotification(
                    (await _context.Orders.FindAsync(id))!.ConsumerId,
                    "Order Cancelled",
                    $"Your order #{id} has been cancelled.",
                    "Order",
                    id
                );
            }

            TempData["SuccessMessage"] =
                "Order status updated successfully.";

            return RedirectToAction(
                "FarmerOrderDetails",
                new { id }
            );
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> MyOrders(string? status)
        {
            int? consumerId =
                HttpContext.Session.GetInt32("UserId");

            if (consumerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Product)
                .Include(o => o.Items)
                .Where(o =>
                    o.ConsumerId == consumerId.Value
                );

            string[] allowedStatuses = { "Pending", "Confirmed", "Completed", "Cancelled" };
            if (!string.IsNullOrWhiteSpace(status) && allowedStatuses.Contains(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            ViewBag.Status = status;

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> ConsumerOrderDetails(
     int id)
        {
            int? consumerId =
                HttpContext.Session.GetInt32("UserId");

            if (consumerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            Order? order = await _context.Orders
                .Include(o => o.Product)
                .ThenInclude(p => p!.Farmer)
                .Include(o => o.Items)
                .Include(o => o.StatusHistory)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.ConsumerId == consumerId.Value
                );

            if (order == null)
            {
                return NotFound();
            }
            bool hasReviewed =
    await _context.Reviews.AnyAsync(r =>
        r.ConsumerId == consumerId.Value &&
        r.ProductId == order.ProductId
    );

            ViewBag.HasReviewed = hasReviewed;

            return View(order);
        }
        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsumerCancelOrder(
    int id, string? cancellationReason = null)
        {
            int? consumerId =
                HttpContext.Session.GetInt32("UserId");

            if (consumerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            Order? order = await _context.Orders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.ConsumerId == consumerId.Value
                );

            if (order == null)
            {
                return NotFound();
            }

            var result=await _workflow.TransitionAsync(id,consumerId.Value,false,"Cancelled",string.IsNullOrWhiteSpace(cancellationReason)?"Cancelled by buyer":cancellationReason.Trim(),$"buyer:{id}:cancel");
            if(!result.Succeeded) { TempData["ErrorMessage"]=result.Message; return RedirectToAction("ConsumerOrderDetails",new{id}); }
            if (order.Product != null)
            {
                await CreateNotification(
                    order.Product.FarmerId,
                    "Order Cancelled by Consumer",
                    $"Order #{order.Id} was cancelled by the Consumer.",
                    "Order",
                    order.Id
                );
            }

            TempData["SuccessMessage"] =
                "Order cancelled successfully.";

            return RedirectToAction(
                "ConsumerOrderDetails",
                new { id = order.Id }
            );
        }

        private async Task CreateNotification(
        int userId,
        string title,
        string message,
        string type,
        int? relatedId = null)
        {
            Notification notification =
                new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RelatedId = relatedId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();
        }
    }
}

