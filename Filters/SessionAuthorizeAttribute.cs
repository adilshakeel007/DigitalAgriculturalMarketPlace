using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DigitalAgriculturalMarketplace.Filters
{
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(
            ActionExecutingContext context)
        {
            int? userId =
                context.HttpContext.Session.GetInt32("UserId");

            string? userType =
                context.HttpContext.Session.GetString("UserType");

            // User is not logged in
            if (userId == null)
            {
                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    null
                );

                return;
            }

            // User is logged in but has wrong role
            if (!string.IsNullOrEmpty(_requiredRole) &&
                userType != _requiredRole)
            {
                context.Result = new RedirectToActionResult(
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
