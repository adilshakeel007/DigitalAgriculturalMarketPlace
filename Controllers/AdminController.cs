using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketPlace.Controllers
{
    [SessionAuthorize(UserRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(
            ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            int totalUsers =
                await _context.Users
                    .CountAsync(u =>
                        u.UserType != UserRoles.Admin
                    );

            int totalFarmers =
                await _context.Users
                    .CountAsync(u =>
                        u.UserType == UserRoles.Farmer
                    );

            int totalConsumers =
                await _context.Users
                    .CountAsync(u =>
                        u.UserType == UserRoles.Consumer
                    );

            int activeUsers =
                await _context.Users
                    .CountAsync(u =>
                        u.UserType != UserRoles.Admin &&
                        u.IsActive
                    );

            int totalProducts =
                await _context.Products.CountAsync();

            int activeProducts =
                await _context.Products
                    .CountAsync(p => p.IsActive);

            int totalOrders =
                await _context.Orders.CountAsync();

            int pendingOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Pending"
                    );

            int completedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Completed"
                    );

            decimal totalRevenue =
                await _context.Orders
                    .Where(o =>
                        o.Status == "Completed"
                    )
                    .SumAsync(o =>
                        (decimal?)o.TotalPrice
                    )
                ?? 0;

            int totalReviews =
                await _context.Reviews.CountAsync();

            int totalMessages =
                await _context.Messages.CountAsync();

            var recentOrders =
                await _context.Orders
                    .Include(o => o.Product)
                    .Include(o => o.Consumer)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();

            AdminDashboardViewModel model =
                new AdminDashboardViewModel
                {
                    TotalUsers = totalUsers,
                    TotalFarmers = totalFarmers,
                    TotalConsumers = totalConsumers,
                    ActiveUsers = activeUsers,
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    CompletedOrders = completedOrders,
                    TotalRevenue = totalRevenue,
                    TotalReviews = totalReviews,
                    TotalMessages = totalMessages,
                    RecentOrders = recentOrders
                };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Users(
            string? searchTerm,
            string? userType,
            string? status)
        {
            IQueryable<User> usersQuery = _context.Users
                .Where(u =>
                    u.UserType != UserRoles.Admin
                );

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    u.Phone.Contains(searchTerm));
            }

            if (userType == UserRoles.Farmer || userType == UserRoles.Consumer)
            {
                usersQuery = usersQuery.Where(u => u.UserType == userType);
            }

            if (status == "active")
            {
                usersQuery = usersQuery.Where(u => u.IsActive);
            }
            else if (status == "inactive")
            {
                usersQuery = usersQuery.Where(u => !u.IsActive);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.UserType = userType;
            ViewBag.Status = status;

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(
    int id)
        {
            User? user =
                await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Id == id &&
                        u.UserType != UserRoles.Admin
                    );

            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                user.IsActive
                    ? "User activated successfully."
                    : "User deactivated successfully.";

            return RedirectToAction("Users");
        }
        [HttpGet]
        public async Task<IActionResult> Products(
            string? searchTerm,
            string? category,
            string? status)
        {
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Farmer)
                .Include(p => p.UnitOfMeasure);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    (p.Farmer != null && p.Farmer.Name.Contains(searchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            if (status == "active")
            {
                productsQuery = productsQuery.Where(p => p.IsActive);
            }
            else if (status == "inactive")
            {
                productsQuery = productsQuery.Where(p => !p.IsActive);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Category = category;
            ViewBag.Status = status;

            var products = await productsQuery
                .OrderByDescending(p => p.ListedAt)
                .ToListAsync();

            return View(products);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleProductStatus(
    int id)
        {
            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == id
                    );

            if (product == null)
            {
                return NotFound();
            }

            product.IsActive =
                !product.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                product.IsActive
                    ? "Product activated successfully."
                    : "Product deactivated successfully.";

            return RedirectToAction("Products");
        }
        [HttpGet]
        public async Task<IActionResult> Orders(
            string? searchTerm,
            string? status)
        {
            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Product)
                    .ThenInclude(p => p!.Farmer)
                .Include(o => o.Consumer);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.Product != null && o.Product.Name.Contains(searchTerm) ||
                    o.Consumer != null && o.Consumer.Name.Contains(searchTerm));
            }

            string[] allowedStatuses = { "Pending", "Confirmed", "Completed", "Cancelled" };
            if (!string.IsNullOrWhiteSpace(status) && allowedStatuses.Contains(status))
            {
                ordersQuery = ordersQuery.Where(o => o.Status == status);
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.Status = status;

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
        [HttpGet]
        public async Task<IActionResult> Reports()
        {
            DateTime startOfMonth =
                new DateTime(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    1
                );

            int totalUsers =
                await _context.Users
                    .CountAsync(u =>
                        u.UserType != UserRoles.Admin
                    );

            int newUsersThisMonth =
                await _context.Users
                    .CountAsync(u =>
                        u.UserType != UserRoles.Admin &&
                        u.CreatedAt >= startOfMonth
                    );

            int totalOrders =
                await _context.Orders.CountAsync();

            int pendingOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Pending"
                    );

            int confirmedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Confirmed"
                    );

            int completedOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Completed"
                    );

            int cancelledOrders =
                await _context.Orders
                    .CountAsync(o =>
                        o.Status == "Cancelled"
                    );

            decimal totalRevenue =
                await _context.Orders
                    .Where(o =>
                        o.Status == "Completed"
                    )
                    .SumAsync(o =>
                        (decimal?)o.TotalPrice
                    )
                ?? 0;

            decimal revenueThisMonth =
                await _context.Orders
                    .Where(o =>
                        o.Status == "Completed" &&
                        o.OrderDate >= startOfMonth
                    )
                    .SumAsync(o =>
                        (decimal?)o.TotalPrice
                    )
                ?? 0;


            var topProducts =
                await _context.Orders

                    .Where(o =>
                        o.Status == "Completed"
                    )

                    .GroupBy(o => new
                    {
                        o.ProductId,
                        ProductName =
                            o.Product != null
                                ? o.Product.Name
                                : "Unknown"
                    })

                    .Select(g =>
                        new TopProductReportItem
                        {
                            ProductId =
                                g.Key.ProductId,

                            ProductName =
                                g.Key.ProductName,

                            TotalQuantitySold =
                                g.Sum(x => x.Quantity),

                            Revenue =
                                g.Sum(x => x.TotalPrice)
                        })

                    .OrderByDescending(x =>
                        x.TotalQuantitySold
                    )

                    .Take(5)

                    .ToListAsync();


            var topFarmers =
                await _context.Orders

                    .Where(o =>
                        o.Status == "Completed" &&
                        o.Product != null &&
                        o.Product.Farmer != null
                    )

                    .GroupBy(o => new
                    {
                        FarmerId =
                            o.Product!.FarmerId,

                        FarmerName =
                            o.Product.Farmer!.Name
                    })

                    .Select(g =>
                        new TopFarmerReportItem
                        {
                            FarmerId =
                                g.Key.FarmerId,

                            FarmerName =
                                g.Key.FarmerName,

                            CompletedOrders =
                                g.Count(),

                            Revenue =
                                g.Sum(x => x.TotalPrice)
                        })

                    .OrderByDescending(x =>
                        x.Revenue
                    )

                    .Take(5)

                    .ToListAsync();


            AdminReportsViewModel model =
                new AdminReportsViewModel
                {
                    TotalUsers = totalUsers,

                    NewUsersThisMonth =
                        newUsersThisMonth,

                    TotalOrders = totalOrders,

                    PendingOrders = pendingOrders,

                    ConfirmedOrders = confirmedOrders,

                    CompletedOrders = completedOrders,

                    CancelledOrders = cancelledOrders,

                    TotalRevenue = totalRevenue,

                    RevenueThisMonth =
                        revenueThisMonth,

                    TopProducts = topProducts,

                    TopFarmers = topFarmers
                };


            return View(model);
        }
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
