using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketPlace.Controllers
{
    public class CatalogProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = CurrentUserId();
            var proposals = await _context.CatalogProducts
                .Include(c => c.DefaultUnit)
                .Where(c => c.SubmittedByUserId == userId)
                .OrderByDescending(c => c.SubmittedAt ?? c.CreatedAt)
                .ToListAsync();
            return View(proposals);
        }

        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpGet]
        public async Task<IActionResult> Create()
            => View(await ProposalModel());

        [SessionAuthorize(UserRoles.MarketplaceMember)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CatalogProductProposalViewModel model)
        {
            int userId = CurrentUserId();
            model.Name = model.Name.Trim();
            model.Category = model.Category.Trim();
            model.Description = model.Description?.Trim();

            bool validUnit = model.DefaultUnitId.HasValue &&
                await _context.UnitsOfMeasure.AnyAsync(u =>
                    u.Id == model.DefaultUnitId.Value && u.IsActive);
            if (!validUnit)
            {
                ModelState.AddModelError(nameof(model.DefaultUnitId), "Select an active unit of measure.");
            }

            var duplicate = await _context.CatalogProducts
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Name == model.Name &&
                    c.Category == model.Category &&
                    c.DefaultUnitId == model.DefaultUnitId &&
                    (c.Status == "Pending" || c.Status == "Approved"));

            if (duplicate != null)
            {
                if (duplicate.SubmittedByUserId == userId && duplicate.Status == "Pending")
                {
                    TempData["InfoMessage"] = "That proposal is already awaiting review.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", duplicate.Status == "Approved"
                    ? "An approved catalog product already matches this proposal."
                    : "A matching proposal is already awaiting review.");
            }

            if (!ModelState.IsValid)
            {
                model.Units = await ActiveUnits();
                return View(model);
            }

            var now = DateTime.UtcNow;
            _context.CatalogProducts.Add(new CatalogProduct
            {
                Name = model.Name,
                Category = model.Category,
                Description = model.Description,
                DefaultUnitId = model.DefaultUnitId,
                Status = "Pending",
                IsActive = false,
                SubmittedByUserId = userId,
                SubmittedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "A matching proposal was submitted at the same time. Review your proposals before trying again.");
                model.Units = await ActiveUnits();
                return View(model);
            }

            TempData["SuccessMessage"] = "Catalog proposal submitted for review.";
            return RedirectToAction(nameof(Index));
        }

        [SessionAuthorize(UserRoles.Admin)]
        [HttpGet]
        public async Task<IActionResult> Moderation(string? status)
        {
            string[] statuses = { "Pending", "Approved", "Rejected" };
            var query = _context.CatalogProducts
                .Include(c => c.DefaultUnit)
                .Include(c => c.SubmittedByUser)
                .Include(c => c.ReviewedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && statuses.Contains(status))
            {
                query = query.Where(c => c.Status == status);
            }

            ViewBag.Status = status;
            return View(await query
                .OrderBy(c => c.Status == "Pending" ? 0 : 1)
                .ThenByDescending(c => c.SubmittedAt ?? c.CreatedAt)
                .ToListAsync());
        }

        [SessionAuthorize(UserRoles.Admin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Moderate(
            CatalogModerationViewModel model,
            string decision)
        {
            if (decision != "Approved" && decision != "Rejected")
            {
                return BadRequest();
            }

            var product = await _context.CatalogProducts
                .FirstOrDefaultAsync(c => c.Id == model.Id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.Status != "Pending")
            {
                TempData["ErrorMessage"] = "This proposal has already been reviewed.";
                return RedirectToAction(nameof(Moderation));
            }

            _context.Entry(product).Property(p => p.RowVersion)
                .OriginalValue = model.RowVersion;
            product.Status = decision;
            product.IsActive = decision == "Approved";
            product.ReviewedByUserId = CurrentUserId();
            product.ReviewedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            product.ReviewNotes = model.ReviewNotes?.Trim();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "This proposal was reviewed by another administrator. The latest result has been preserved.";
                return RedirectToAction(nameof(Moderation));
            }

            TempData["SuccessMessage"] = $"Catalog proposal {decision.ToLowerInvariant()}.";
            return RedirectToAction(nameof(Moderation));
        }

        private int CurrentUserId()
            => HttpContext.Session.GetInt32("UserId")!.Value;

        private async Task<CatalogProductProposalViewModel> ProposalModel()
            => new() { Units = await ActiveUnits() };

        private Task<List<UnitOfMeasure>> ActiveUnits()
            => _context.UnitsOfMeasure
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();
    }
}
