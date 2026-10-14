using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketPlace.Data;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DigitalAgriculturalMarketPlace.Controllers
{
    [SessionAuthorize(UserRoles.Consumer)]
    public class ReviewsController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("MyOrders", "Orders");
        }
        private readonly ApplicationDbContext _context;

        public ReviewsController(
            ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Create(
     int productId)
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
                    .FirstOrDefaultAsync(p =>
                        p.Id == productId
                    );

            if (product == null)
            {
                return NotFound();
            }

            bool hasCompletedOrder =
                await _context.Orders.AnyAsync(o =>
                    o.ConsumerId == consumerId.Value &&
                    o.ProductId == productId &&
                    o.Status == "Completed"
                );

            if (!hasCompletedOrder)
            {
                TempData["ErrorMessage"] =
                    "You can review this product only after completing an order.";

                return RedirectToAction(
                    "MyOrders",
                    "Orders"
                );
            }

            bool alreadyReviewed =
                await _context.Reviews.AnyAsync(r =>
                    r.ConsumerId == consumerId.Value &&
                    r.ProductId == productId
                );

            if (alreadyReviewed)
            {
                TempData["ErrorMessage"] =
                    "You have already reviewed this product.";

                return RedirectToAction(
                    "MyOrders",
                    "Orders"
                );
            }

            CreateReviewViewModel model =
                new CreateReviewViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name
                };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    CreateReviewViewModel model)
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
                    .FirstOrDefaultAsync(p =>
                        p.Id == model.ProductId
                    );

            if (product == null)
            {
                return NotFound();
            }

            model.ProductName = product.Name;

            bool hasCompletedOrder =
                await _context.Orders.AnyAsync(o =>
                    o.ConsumerId == consumerId.Value &&
                    o.ProductId == model.ProductId &&
                    o.Status == "Completed"
                );

            if (!hasCompletedOrder)
            {
                ModelState.AddModelError(
                    "",
                    "You can review this product only after completing an order."
                );

                return View(model);
            }

            bool alreadyReviewed =
                await _context.Reviews.AnyAsync(r =>
                    r.ConsumerId == consumerId.Value &&
                    r.ProductId == model.ProductId
                );

            if (alreadyReviewed)
            {
                ModelState.AddModelError(
                    "",
                    "You have already reviewed this product."
                );

                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Review review = new Review
            {
                ConsumerId = consumerId.Value,
                ProductId = model.ProductId,
                Rating = model.Rating,
                Comment = model.Comment?.Trim(),
                ReviewDate = DateTime.Now
            };

            _context.Reviews.Add(review);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Review submitted successfully.";

            return RedirectToAction(
                "Details",
                "Marketplace",
                new { id = model.ProductId }
            );
        }
    }

}
