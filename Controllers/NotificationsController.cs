using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketplace.Controllers
{
    [SessionAuthorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var notifications =
                await _context.Notifications
                    .Where(n => n.UserId == userId.Value)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();

            return View(notifications);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(
    int id)
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            Notification? notification =
                await _context.Notifications
                    .FirstOrDefaultAsync(n =>
                        n.Id == id &&
                        n.UserId == userId.Value
                    );

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int? userId =
                HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
            }

            var notifications =
                await _context.Notifications

                    .Where(n =>
                        n.UserId == userId.Value &&
                        !n.IsRead
                    )

                    .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
