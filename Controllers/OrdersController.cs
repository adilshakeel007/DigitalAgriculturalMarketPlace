using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DigitalAgriculturalMarketPlace.Controllers
{

    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }
        [SessionAuthorize(UserRoles.Consumer)]
        [HttpGet]
        public async Task<IActionResult> PlaceOrder(int productId)
        {
            Product? product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.Id == productId &&
                    p.IsActive &&
                    p.Quantity > 0
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
                    AvailableQuantity = product.Quantity,
                    Quantity = 1
                };

            return View(model);

        }
        [SessionAuthorize(UserRoles.Consumer)]
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

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == model.ProductId
                    );

            if (product == null)
            {
                return NotFound();
            }

            // Refill display-only values
            model.ProductName = product.Name;
            model.PricePerUnit = product.PricePerUnit;
            model.AvailableQuantity = product.Quantity;

            if (!product.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "This product is no longer available."
                );

                return View(model);
            }

            if (product.Quantity <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "This product is out of stock."
                );

                return View(model);
            }

            if (model.Quantity <= 0)
            {
                ModelState.AddModelError(
                    "Quantity",
                    "Quantity must be at least 1."
                );

                return View(model);
            }

            if (model.Quantity > product.Quantity)
            {
                ModelState.AddModelError(
                    "Quantity",
                    $"Only {product.Quantity} unit(s) are available."
                );

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            decimal totalPrice =
                product.PricePerUnit *
                model.Quantity;

            Order order = new Order
            {
                ConsumerId = consumerId.Value,
                ProductId = product.Id,
                Quantity = model.Quantity,
                TotalPrice = totalPrice,
                Status = "Pending",
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);

            product.Quantity -= model.Quantity;

            await _context.SaveChangesAsync();
            await CreateNotification(
    product.FarmerId,
    "New Order",
    $"A new order has been placed for {product.Name}.",
    "Order",
    order.Id
);

            await transaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Your order has been placed successfully.";

            return RedirectToAction(
                "OrderSuccess",
                new { id = order.Id }
            );
        }
        [SessionAuthorize(UserRoles.Consumer)]
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
        [SessionAuthorize(UserRoles.Farmer)]
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
        [SessionAuthorize(UserRoles.Farmer)]
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
        [SessionAuthorize(UserRoles.Farmer)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(
     int id,
     string status)
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

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

            Order? order = await _context.Orders
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o =>
                    o.Id == id &&
                    o.Product != null &&
                    o.Product.FarmerId == farmerId.Value
                );

            if (order == null)
            {
                return NotFound();
            }


            // Prevent changing completed/cancelled orders
            if (order.Status == "Completed" ||
                order.Status == "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "This order can no longer be changed.";

                return RedirectToAction(
                    "FarmerOrderDetails",
                    new { id = order.Id }
                );
            }


            // PENDING -> CONFIRMED
            if (order.Status == "Pending" &&
                status == "Confirmed")
            {
                order.Status = "Confirmed";
            }


            // CONFIRMED -> COMPLETED
            else if (order.Status == "Confirmed" &&
                     status == "Completed")
            {
                order.Status = "Completed";
            }


            // PENDING / CONFIRMED -> CANCELLED
            else if (
                (order.Status == "Pending" ||
                 order.Status == "Confirmed") &&
                status == "Cancelled")
            {
                // Restore stock ONLY ONCE
                if (order.Product != null)
                {
                    order.Product.Quantity += order.Quantity;
                }

                // Very important
                order.Status = "Cancelled";
            }


            // Invalid status change
            else
            {
                TempData["ErrorMessage"] =
                    "Invalid order status change.";

                return RedirectToAction(
                    "FarmerOrderDetails",
                    new { id = order.Id }
                );
            }


            await _context.SaveChangesAsync();
            if (status == "Confirmed")
            {
                await CreateNotification(
                    order.ConsumerId,
                    "Order Confirmed",
                    $"Your order #{order.Id} has been confirmed.",
                    "Order",
                    order.Id
                );
            }
            if (status == "Completed")
            {
                await CreateNotification(
                    order.ConsumerId,
                    "Order Completed",
                    $"Your order #{order.Id} has been completed.",
                    "Order",
                    order.Id
                );
            }
            if (status == "Cancelled")
            {
                await CreateNotification(
                    order.ConsumerId,
                    "Order Cancelled",
                    $"Your order #{order.Id} has been cancelled.",
                    "Order",
                    order.Id
                );
            }

            await transaction.CommitAsync();


            TempData["SuccessMessage"] =
                "Order status updated successfully.";

            return RedirectToAction(
                "FarmerOrderDetails",
                new { id = order.Id }
            );
        }
        [SessionAuthorize(UserRoles.Consumer)]
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
        [SessionAuthorize(UserRoles.Consumer)]
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
        [SessionAuthorize(UserRoles.Consumer)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConsumerCancelOrder(
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

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable
                );

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

            if (order.Status != "Pending")
            {
                TempData["ErrorMessage"] =
                    "Only pending orders can be cancelled.";

                return RedirectToAction(
                    "ConsumerOrderDetails",
                    new { id = order.Id }
                );
            }
            if (order.Product != null)
            {
                order.Product.Quantity += order.Quantity;
            }
            else
            {
                return NotFound();
            }

            order.Status = "Cancelled";

            await _context.SaveChangesAsync();
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

            await transaction.CommitAsync();

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

