using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketplace.Controllers
{
    [SessionAuthorize(UserRoles.Consumer)]
    public class MarketplaceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MarketplaceController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(
    string? searchTerm,
    string? category,
    decimal? minPrice,
    decimal? maxPrice,
    string? sortBy)
        {
            var products = _context.Products
                .Include(p => p.Farmer)
                .Where(p =>
                    p.IsActive &&
                    p.Quantity > 0
                )
                .AsQueryable();


            // SEARCH
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchTerm)
                );
            }


            // CATEGORY
            if (!string.IsNullOrWhiteSpace(category))
            {
                products = products.Where(p =>
                    p.Category == category
                );
            }


            // PRICE RANGE VALIDATION
            bool invalidPriceRange =
                minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice.Value > maxPrice.Value;

            if (invalidPriceRange)
            {
                ViewBag.PriceError =
                    "Minimum price cannot be greater than maximum price.";
            }
            else
            {
                if (minPrice.HasValue)
                {
                    products = products.Where(p =>
                        p.PricePerUnit >= minPrice.Value
                    );
                }

                if (maxPrice.HasValue)
                {
                    products = products.Where(p =>
                        p.PricePerUnit <= maxPrice.Value
                    );
                }
            }


            // SORTING
            switch (sortBy)
            {
                case "price_low":

                    products =
                        products.OrderBy(
                            p => p.PricePerUnit
                        );

                    break;


                case "price_high":

                    products =
                        products.OrderByDescending(
                            p => p.PricePerUnit
                        );

                    break;


                case "oldest":

                    products =
                        products.OrderBy(
                            p => p.ListedAt
                        );

                    break;


                default:

                    products =
                        products.OrderByDescending(
                            p => p.ListedAt
                        );

                    break;
            }


            // REMEMBER FILTER VALUES
            ViewBag.SearchTerm = searchTerm;

            ViewBag.Category = category;

            ViewBag.MinPrice = minPrice;

            ViewBag.MaxPrice = maxPrice;

            ViewBag.SortBy = sortBy;


            var result =
                await products.ToListAsync();

            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Product? product =
    await _context.Products
        .Include(p => p.Farmer)
        .Include(p => p.Reviews)
            .ThenInclude(r => r.Consumer)
        .FirstOrDefaultAsync(p =>
            p.Id == id &&
            p.IsActive &&
            p.Quantity > 0
        );

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}