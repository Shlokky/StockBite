using Microsoft.AspNetCore.Mvc;
using StockBite.Models;
using StockBite.Services;
namespace StockBite.Controllers
{
    public class BaseController : Controller
    {
        private readonly IRoleContext _roleContext;

        protected BaseController(IRoleContext roleContext)
        {
            _roleContext = roleContext;
        }

        protected UserRole CurrentUserRole => _roleContext.CurrentRole;
        protected int? CurrentVendorId => _roleContext.CurrentVendorId;
        protected int? CurrentConsumerId => _roleContext.CurrentConsumerId;
        protected bool IsAuthenticated => _roleContext.IsAuthenticated;

        protected bool IsAuthorized(UserRole requiredRole)
        {
            if (requiredRole == UserRole.Public)
            {
                return true;
            }

            return CurrentUserRole == requiredRole && IsAuthenticated;
        }

        protected IActionResult RedirectToUnauthorized()
        {
            TempData["ErrorMessage"] = "You are not authorized to view this page.";
            return RedirectToAction("Index", "Home");
        }
    }
}
