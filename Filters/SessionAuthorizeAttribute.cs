using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DigitalAgriculturalMarketplace.Constants;
using DigitalAgriculturalMarketPlace.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalAgriculturalMarketplace.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(
            string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            int? userId =
                context.HttpContext.Session
                    .GetInt32("UserId");

            if (userId == null)
            {
                context.Result =
                    new RedirectToActionResult(
                        "Login",
                        "Account",
                        null
                    );

                return;
            }

            ApplicationDbContext? dbContext =
                context.HttpContext.RequestServices
                    .GetService<ApplicationDbContext>();

            var user = dbContext?.Users
                .FirstOrDefault(u => u.Id == userId.Value);

            if (user == null || !user.IsActive)
            {
                context.HttpContext.Session.Clear();
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null);
                return;
            }

            context.HttpContext.Session.SetString("UserName", user.Name);
            context.HttpContext.Session.SetString("UserType", user.UserType);

            bool isAuthorized = string.IsNullOrEmpty(_requiredRole)
                || (string.Equals(_requiredRole, UserRoles.MarketplaceMember,
                        StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(user.UserType, UserRoles.Admin,
                        StringComparison.OrdinalIgnoreCase))
                || string.Equals(user.UserType, _requiredRole,
                    StringComparison.OrdinalIgnoreCase);

            if (!isAuthorized)
            {
                context.Result =
                    new RedirectToActionResult(
                        "AccessDenied",
                        "Account",
                        null
                    );

                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
