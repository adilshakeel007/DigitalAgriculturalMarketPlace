using System.Diagnostics;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketPlace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketplace.Filters;
using Microsoft.AspNetCore.Mvc;
using DigitalAgriculturalMarketplace.Constants;
using Microsoft.EntityFrameworkCore;


namespace DigitalAgriculturalMarketPlace.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        [SessionAuthorize(UserRoles.Farmer)]
        public async Task<IActionResult> FarmerDashboard()
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

            int totalProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.FarmerId == farmerId.Value
                    );

            int activeProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.FarmerId == farmerId.Value &&
                        p.IsActive
                    );

            int lowStockProducts =
                await _context.Products
                    .CountAsync(p =>
                        p.FarmerId == farmerId.Value &&
                        p.IsActive &&
                        p.Quantity <= 5
                    );

            int totalStockUnits =
                await _context.Products
                    .Where(p =>
                        p.FarmerId == farmerId.Value &&
                        p.IsActive
                    )
                    .SumAsync(p => (int?)p.Quantity)
                ?? 0;

            int pendingOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Product != null &&
                        o.Product.FarmerId == farmerId.Value &&
                        o.Status == "Pending"
                    );

            int confirmedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Product != null &&
                        o.Product.FarmerId == farmerId.Value &&
                        o.Status == "Confirmed"
                    );

            int completedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Product != null &&
                        o.Product.FarmerId == farmerId.Value &&
                        o.Status == "Completed"
                    );

            decimal totalRevenue =
                await _context.Orders
                    .Where(o =>
                        o.Product != null &&
                        o.Product.FarmerId == farmerId.Value &&
                        o.Status == "Completed"
                    )
                    .SumAsync(o => (decimal?)o.TotalPrice)
                    ?? 0;

            int unreadMessages =
                await _context.Messages
                    .CountAsync(m =>
                        m.ReceiverId == farmerId.Value &&
                        !m.IsRead
                    );
            int unreadNotifications =
    await _context.Notifications
        .CountAsync(n =>
            n.UserId == farmerId.Value &&
            !n.IsRead
        );

            var recentOrders =
                await _context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Consumer)
                    .Where(o =>
                        o.Product != null &&
                        o.Product.FarmerId == farmerId.Value
                    )
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

            var inventoryOverview =
                await _context.Products
                    .Where(p => p.FarmerId == farmerId.Value)
                    .OrderBy(p => p.Quantity)
                    .ThenBy(p => p.Name)
                    .Take(6)
                    .ToListAsync();

            FarmerDashboardViewModel model =
                new FarmerDashboardViewModel
                {
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    LowStockProducts = lowStockProducts,
                    TotalStockUnits = totalStockUnits,
                    PendingOrders = pendingOrders,
                    ConfirmedOrders = confirmedOrders,
                    CompletedOrders = completedOrders,
                    TotalRevenue = totalRevenue,
                    UnreadMessages = unreadMessages,
                    UnreadNotifications = unreadNotifications,
                    RecentOrders = recentOrders,
                    InventoryOverview = inventoryOverview
                };

            return View(model);
        }
        [SessionAuthorize(UserRoles.Consumer)]
        public async Task<IActionResult> ConsumerDashboard()
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

            int totalOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.ConsumerId == consumerId.Value
                    );

            int pendingOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.ConsumerId == consumerId.Value &&
                        o.Status == "Pending"
                    );

            int confirmedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.ConsumerId == consumerId.Value &&
                        o.Status == "Confirmed"
                    );

            int completedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.ConsumerId == consumerId.Value &&
                        o.Status == "Completed"
                    );

            int cancelledOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.ConsumerId == consumerId.Value &&
                        o.Status == "Cancelled"
                    );

            int unreadMessages =
                await _context.Messages
                    .CountAsync(m =>
                        m.ReceiverId == consumerId.Value &&
                        !m.IsRead
                    );
            int unreadNotifications =
    await _context.Notifications
        .CountAsync(n =>
            n.UserId == consumerId.Value &&
            !n.IsRead
        );

            var recentOrders =
                await _context.Orders
                    .Include(o => o.Product)
                    .ThenInclude(p => p!.Farmer)
                    .Where(o =>
                        o.ConsumerId == consumerId.Value
                    )
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

            var activeOrders =
                await _context.Orders
                    .Include(o => o.Product)
                    .ThenInclude(p => p!.Farmer)
                    .Where(o =>
                        o.ConsumerId == consumerId.Value &&
                        (o.Status == "Pending" || o.Status == "Confirmed")
                    )
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

            var orderHistory =
                await _context.Orders
                    .Include(o => o.Product)
                    .ThenInclude(p => p!.Farmer)
                    .Where(o =>
                        o.ConsumerId == consumerId.Value &&
                        (o.Status == "Completed" || o.Status == "Cancelled")
                    )
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

            ConsumerDashboardViewModel model =
                new ConsumerDashboardViewModel
                {
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    ConfirmedOrders = confirmedOrders,
                    CompletedOrders = completedOrders,
                    CancelledOrders = cancelledOrders,
                    UnreadMessages = unreadMessages,
                    UnreadNotifications = unreadNotifications,
                    RecentOrders = recentOrders,
                    ActiveOrders = activeOrders,
                    OrderHistory = orderHistory
                };

            return View(model);
        }
        [SessionAuthorize]
        public IActionResult Profile()
        {
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Index()
        {
            string? userType = HttpContext.Session.GetString("UserType");

            if (userType == UserRoles.Farmer)
            {
                return RedirectToAction(nameof(FarmerDashboard));
            }

            if (userType == UserRoles.Consumer)
            {
                return RedirectToAction(nameof(ConsumerDashboard));
            }

            if (userType == UserRoles.Admin)
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
