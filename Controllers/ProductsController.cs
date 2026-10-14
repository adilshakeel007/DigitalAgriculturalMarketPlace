using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalAgriculturalMarketplace.Services;
using Microsoft.Extensions.DependencyInjection;


namespace DigitalAgriculturalMarketplace.Controllers
{
    [SessionAuthorize(UserRoles.MarketplaceMember)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly InventoryService _inventory;

        public ProductsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment) : this(context, environment, new InventoryService(context)) { }
        [ActivatorUtilitiesConstructor]
        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment, InventoryService inventory)
        {
            _context = context;
            _environment = environment;
            _inventory = inventory;
        }
        public async Task<IActionResult> Index()
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            var products = await _context.Products
                .Include(p => p.UnitOfMeasure)
                .Where(p => p.FarmerId == farmerId)
                .OrderByDescending(p => p.ListedAt)
                .ToListAsync();

            return View(products);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    ProductCreateViewModel model)
        {
            int? farmerId = HttpContext.Session.GetInt32("UserId");
            if (farmerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            bool canSell = await _context.SellerProfiles.AnyAsync(s =>
                s.UserId == farmerId.Value && s.IsActive);
            if (!canSell)
            {
                TempData["ErrorMessage"] = "Activate your seller profile before creating a listing.";
                return RedirectToAction("Activate", "SellerProfiles");
            }

            bool approvedCatalogProduct = model.CatalogProductId.HasValue &&
                await _context.CatalogProducts.AnyAsync(c =>
                    c.Id == model.CatalogProductId.Value &&
                    c.Status == "Approved" && c.IsActive);
            if (!approvedCatalogProduct)
            {
                ModelState.AddModelError(nameof(model.CatalogProductId), "Select an approved catalog product.");
            }

            bool activeUnit = model.UnitOfMeasureId.HasValue &&
                await _context.UnitsOfMeasure.AnyAsync(u =>
                    u.Id == model.UnitOfMeasureId.Value && u.IsActive);
            if (!activeUnit)
            {
                ModelState.AddModelError(nameof(model.UnitOfMeasureId), "Select an active unit of measure.");
            }

            await PopulateListingOptions(model);
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string? imageUrl = null;

            if (model.ImageFile != null &&
                model.ImageFile.Length > 0)
            {
                string[] allowedExtensions =
                {
            ".jpg",
            ".jpeg",
            ".png"
        };

                string extension =
                    Path.GetExtension(
                        model.ImageFile.FileName
                    ).ToLower();

                string[] allowedContentTypes =
                {
                    "image/jpeg",
                    "image/png"
                };

                if (!allowedExtensions.Contains(extension) ||
                    !allowedContentTypes.Contains(model.ImageFile.ContentType))
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Please upload a valid JPG or PNG image."
                    );
                    return View(model);
                }

                long maxFileSize =
                    5 * 1024 * 1024;

                if (model.ImageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Image size cannot exceed 5 MB."
                    );

                    return View(model);
                }

                string uniqueFileName =
                    Guid.NewGuid().ToString()
                    + extension;

                string uploadFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "products"
                    );

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        uniqueFileName
                    );

                using (FileStream stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await model.ImageFile
                        .CopyToAsync(stream);
                }

                imageUrl =
                    "/images/products/"
                    + uniqueFileName;
            }

            Product product = new Product
            {
                Name = model.Name.Trim(),

                Category = model.Category,

                PricePerUnit = model.PricePerUnit,

                Quantity = model.Quantity,

                Description = model.Description,

                ImageUrl = imageUrl,

                FarmerId = farmerId.Value,

                CatalogProductId = model.CatalogProductId,

                UnitOfMeasureId = model.UnitOfMeasureId,

                AvailableQuantity = model.Quantity,

                ReservedQuantity = 0,

                ListedAt = DateTime.Now,

                IsActive = true
            };

            await using var inventoryTransaction = await _context.Database.BeginTransactionAsync();
            _context.Products.Add(product);

            await _context.SaveChangesAsync();
            _inventory.AddMovement(product, "OpeningBalance", model.Quantity, 0, farmerId.Value, "Product", product.Id, "Initial listing stock", $"opening:product:{product.Id}");
            await _context.SaveChangesAsync();
            await inventoryTransaction.CommitAsync();

            TempData["SuccessMessage"] =
                "Product added successfully.";

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.FarmerId == farmerId
                    );

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
     int id,
     ProductEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.FarmerId == farmerId
                    );

            if (product == null)
            {
                return NotFound();
            }

            product.Name = model.Name.Trim();
            product.Category = model.Category;
            product.PricePerUnit = model.PricePerUnit;
            product.Description = model.Description;
            product.IsActive = model.IsActive;

            if (model.NewImageFile != null &&
                model.NewImageFile.Length > 0)
            {
                string[] allowedExtensions =
                {
            ".jpg",
            ".jpeg",
            ".png"
        };

                string extension =
                    Path.GetExtension(
                        model.NewImageFile.FileName
                    ).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "NewImageFile",
                        "Only JPG, JPEG and PNG images are allowed."
                    );

                    model.CurrentImageUrl =
                        product.ImageUrl;

                    return View(model);
                }

                string[] allowedContentTypes =
                {
                    "image/jpeg",
                    "image/png"
                };

                if (!allowedContentTypes.Contains(model.NewImageFile.ContentType))
                {
                    ModelState.AddModelError(
                        "NewImageFile",
                        "Please upload a valid JPG or PNG image."
                    );

                    model.CurrentImageUrl = product.ImageUrl;
                    return View(model);
                }

                long maxFileSize =
                    5 * 1024 * 1024;

                if (model.NewImageFile.Length >
                    maxFileSize)
                {
                    ModelState.AddModelError(
                        "NewImageFile",
                        "Image size cannot exceed 5 MB."
                    );

                    model.CurrentImageUrl =
                        product.ImageUrl;

                    return View(model);
                }

                if (!string.IsNullOrEmpty(
                    product.ImageUrl))
                {
                    string oldImagePath =
                        Path.Combine(
                            _environment.WebRootPath,
                            product.ImageUrl
                                .TrimStart('/')
                                .Replace(
                                    '/',
                                    Path.DirectorySeparatorChar
                                )
                        );

                    if (System.IO.File.Exists(
                        oldImagePath))
                    {
                        System.IO.File.Delete(
                            oldImagePath
                        );
                    }
                }

                string uniqueFileName =
                    Guid.NewGuid().ToString()
                    + extension;

                string uploadFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "products"
                    );

                Directory.CreateDirectory(uploadFolder);

                string filePath =
                    Path.Combine(
                        uploadFolder,
                        uniqueFileName
                    );

                using (FileStream stream =
                    new FileStream(
                        filePath,
                        FileMode.Create))
                {
                    await model.NewImageFile
                        .CopyToAsync(stream);
                }

                product.ImageUrl =
                    "/images/products/"
                    + uniqueFileName;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Product updated successfully.";

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            if (farmerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.FarmerId == farmerId.Value
                    );

            if (product == null)
            {
                return NotFound();
            }

            ProductEditViewModel model =
                new ProductEditViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    PricePerUnit = product.PricePerUnit,
                    Description = product.Description,
                    IsActive = product.IsActive,
                    CurrentImageUrl = product.ImageUrl
                };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> AdjustInventory(int id)
        {
            int userId=HttpContext.Session.GetInt32("UserId")!.Value;
            var product=await _context.Products.Include(p=>p.UnitOfMeasure).FirstOrDefaultAsync(p=>p.Id==id&&p.FarmerId==userId);
            if(product is null) return NotFound();
            if(product.UnitOfMeasureId is null) { TempData["ErrorMessage"]="Map a unit of measure before adjusting this legacy listing."; return RedirectToAction(nameof(Index)); }
            ViewBag.Movements=await _context.InventoryMovements.Where(m=>m.ProductId==id).OrderByDescending(m=>m.CreatedAt).Take(10).ToListAsync();
            return View(new InventoryAdjustmentViewModel { ProductId=id,ProductName=product.Name,CurrentAvailable=product.AvailableQuantity??product.Quantity,Reserved=product.ReservedQuantity,NewAvailable=product.AvailableQuantity??product.Quantity });
        }
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustInventory(InventoryAdjustmentViewModel model)
        {
            int userId=HttpContext.Session.GetInt32("UserId")!.Value;
            if(!ModelState.IsValid) return View(model);
            var result=await _inventory.AdjustAsync(model.ProductId,userId,model.NewAvailable,model.Reason,$"adjust:{model.OperationKey}");
            if(!result.Succeeded){ModelState.AddModelError("",result.Message);return View(model);}
            TempData["SuccessMessage"]=result.Message; return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.FarmerId == farmerId
                    );

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
      int id)
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

            Product? product =
                await _context.Products
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.FarmerId == farmerId.Value
                    );

            if (product == null)
            {
                return NotFound();
            }

            bool hasOrders =
                await _context.Orders
                    .AnyAsync(o =>
                        o.ProductId == product.Id
                    );

            if (hasOrders)
            {
                product.IsActive = false;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Product has order history and was deactivated instead of deleted.";

                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                string imagePath =
                    Path.Combine(
                        _environment.WebRootPath,
                        product.ImageUrl
                            .TrimStart('/')
                            .Replace(
                                '/',
                                Path.DirectorySeparatorChar
                            )
                    );

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Product deleted successfully.";

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            bool canSell = await _context.SellerProfiles.AnyAsync(s =>
                s.UserId == userId && s.IsActive);
            if (!canSell)
            {
                TempData["ErrorMessage"] = "Activate your seller profile before creating a listing.";
                return RedirectToAction("Activate", "SellerProfiles");
            }

            var model = new ProductCreateViewModel();
            await PopulateListingOptions(model);
            return View(model);
        }

        private async Task PopulateListingOptions(ProductCreateViewModel model)
        {
            model.CatalogProducts = await _context.CatalogProducts
                .Where(c => c.Status == "Approved" && c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            model.Units = await _context.UnitsOfMeasure
                .Where(u => u.IsActive)
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
    }
}
