using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Controllers;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public OrdersController(ApplicationDbContext dbContext, IRoleContext roleContext)
            : base(roleContext)
        {
            _dbContext = dbContext;
        }

        // GET: Orders
        public IActionResult Index()
        {
            if (!IsAuthorized(UserRole.Admin) && !IsAuthorized(UserRole.StockManager) && !IsAuthorized(UserRole.Vendor))
            {
                return RedirectToUnauthorized();
            }

            if (CurrentUserRole == UserRole.Vendor)
            {
                return RedirectToAction("Orders", "Vendors");
            }

            var orders = _dbContext.Orders
                .Include(o => o.Product)
                .Include(o => o.Vendor)
                .Include(o => o.Consumer)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }
    }
}
