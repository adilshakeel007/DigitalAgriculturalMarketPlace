using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalAgriculturalMarketPlace.Controllers
{
    [SessionAuthorize(UserRoles.MarketplaceMember)]
    public class SellerProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SellerProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int userId = CurrentUserId();
            var profile = await _context.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);
            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Activate()
        {
            int userId = CurrentUserId();
            var existing = await _context.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (existing != null)
            {
                TempData["InfoMessage"] = "Your seller profile is already active.";
                return RedirectToAction(nameof(Index));
            }

            string displayName = HttpContext.Session.GetString("UserName") ?? string.Empty;
            return View(new SellerProfileViewModel { DisplayName = displayName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(SellerProfileViewModel model)
        {
            int userId = CurrentUserId();

            if (await _context.SellerProfiles.AnyAsync(s => s.UserId == userId))
            {
                TempData["InfoMessage"] = "Your seller profile already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var profile = new SellerProfile
            {
                UserId = userId,
                DisplayName = model.DisplayName.Trim(),
                Description = model.Description?.Trim(),
                IsActive = true,
                VerificationStatus = "Unverified",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.SellerProfiles.Add(profile);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _context.Entry(profile).State = EntityState.Detached;
                if (await _context.SellerProfiles.AsNoTracking()
                    .AnyAsync(s => s.UserId == userId))
                {
                    TempData["InfoMessage"] = "Your seller profile already exists.";
                    return RedirectToAction(nameof(Index));
                }

                throw;
            }

            TempData["SuccessMessage"] = "Your seller profile is ready.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            int userId = CurrentUserId();
            var profile = await _context.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
            {
                return RedirectToAction(nameof(Activate));
            }

            return View(new SellerProfileViewModel
            {
                DisplayName = profile.DisplayName,
                Description = profile.Description,
                RowVersion = profile.RowVersion
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SellerProfileViewModel model)
        {
            int userId = CurrentUserId();
            var profile = await _context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid || model.RowVersion == null)
            {
                return View(model);
            }

            _context.Entry(profile).Property(p => p.RowVersion)
                .OriginalValue = model.RowVersion;
            profile.DisplayName = model.DisplayName.Trim();
            profile.Description = model.Description?.Trim();
            profile.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "This seller profile was changed elsewhere. Review the latest information and try again.");
                await _context.Entry(profile).ReloadAsync();
                model.RowVersion = profile.RowVersion;
                return View(model);
            }

            TempData["SuccessMessage"] = "Seller profile updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(byte[] rowVersion)
        {
            int userId = CurrentUserId();
            var profile = await _context.SellerProfiles
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (profile == null)
            {
                return NotFound();
            }

            _context.Entry(profile).Property(p => p.RowVersion)
                .OriginalValue = rowVersion;
            profile.IsActive = !profile.IsActive;
            profile.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] = "This seller profile was changed elsewhere. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] = profile.IsActive
                ? "Selling has been activated."
                : "Selling has been paused.";
            return RedirectToAction(nameof(Index));
        }

        private int CurrentUserId()
            => HttpContext.Session.GetInt32("UserId")!.Value;
    }
}
