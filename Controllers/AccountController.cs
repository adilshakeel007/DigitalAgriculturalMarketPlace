using DigitalAgriculturalMarketplace.Models;
using DigitalAgriculturalMarketplace.ViewModels;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalAgriculturalMarketplace.Constants;

namespace DigitalAgriculturalMarketplace.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }


            string normalizedEmail =
                model.Email.Trim().ToLower();


            bool emailExists =
                await _context.Users
                    .AnyAsync(u =>
                        u.Email == normalizedEmail);


            if (emailExists)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists."
                );

                return View(model);
            }


            if (model.UserType != UserRoles.Farmer &&
                model.UserType != UserRoles.Consumer)
            {
                ModelState.AddModelError(
                    "UserType",
                    "Invalid account type."
                );

                return View(model);

            }

            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    model.Password
                );


            User user = new User
            {
                Name = model.Name.Trim(),

                Email = normalizedEmail,

                Phone = model.Phone.Trim(),

                PasswordHash = passwordHash,

                UserType = model.UserType,

                IsActive = true,

                CreatedAt = DateTime.Now
            };


            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            TempData["SuccessMessage"] =
                "Registration successful. Please Login.";


            return RedirectToAction(
                "Login",
                "Account"
            );
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                string? userType =
                    HttpContext.Session.GetString("UserType");

                if (string.Equals(
    userType,
    UserRoles.Admin,
    StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Admin"
                    );
                }

                return RedirectToAction("Dashboard", "Home");
            }

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string normalizedEmail = model.Email.Trim().ToLower();

            User? user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "Your account has been deactivated.");

                return View(model);
            }

            bool passwordIsCorrect = BCrypt.Net.BCrypt.Verify(
                model.Password,
                user.PasswordHash);

            if (!passwordIsCorrect)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("UserType", user.UserType);

            if (user.UserType == UserRoles.Admin)
            {
                return RedirectToAction(
                    "Dashboard",
                    "Admin"
                );
            }

            if (user.UserType != UserRoles.Admin)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Account"
            );
        }
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
