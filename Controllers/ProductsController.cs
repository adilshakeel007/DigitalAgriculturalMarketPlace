using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketplace.Filters;
using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DigitalAgriculturalMarketplace.Controllers
{
    [SessionAuthorize(UserRoles.Farmer)]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        public async Task<IActionResult> Index()
        {
            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            var products = await _context.Products
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int? farmerId =
                HttpContext.Session.GetInt32("UserId");

            if (farmerId == null)
            {
                return RedirectToAction(
                    "Login",
                    "Account"
                );
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

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        "ImageFile",
                        "Only JPG, JPEG and PNG images are allowed."
                    );
                    string[] allowedContentTypes =
                    {
                        "image/jpeg",
                        "image/png"
                    };

                    if (!allowedContentTypes.Contains(
                        model.ImageFile.ContentType))
                    {
                        ModelState.AddModelError(
                            "ImageFile",
                            "The uploaded file is not a valid image."
                        );

                        return View(model);
                    }

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

                ListedAt = DateTime.Now,

                IsActive = true
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

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
            product.Quantity = model.Quantity;
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

            ProductEditViewModel model =
                new ProductEditViewModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = product.Category,
                    PricePerUnit = product.PricePerUnit,
                    Quantity = product.Quantity,
                    Description = product.Description,
                    IsActive = product.IsActive,
                    CurrentImageUrl = product.ImageUrl
                };

            return View(model);
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
        public async Task<IActionResult> DeleteConfirmed(int id)
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
        public IActionResult Create()
        {
            return View();
        }
    }
}