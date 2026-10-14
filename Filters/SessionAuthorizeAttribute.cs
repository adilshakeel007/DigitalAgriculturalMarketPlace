using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

            string? userType =
                context.HttpContext.Session
                    .GetString("UserType");

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

            if (!string.IsNullOrEmpty(_requiredRole) &&
                !string.Equals(
                    userType,
                    _requiredRole,
                    StringComparison.OrdinalIgnoreCase))
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
