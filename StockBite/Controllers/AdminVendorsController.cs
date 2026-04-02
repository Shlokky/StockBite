using Microsoft.AspNetCore.Mvc;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Controllers
{
    public class AdminVendorsController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminVendorsController(ApplicationDbContext dbContext, IRoleContext roleContext)
            : base(roleContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            return View(_dbContext.Vendors.ToList());
        }

        public IActionResult Create()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Vendor vendor)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            vendor.Name = (vendor.Name ?? string.Empty).Trim();

            if (ModelState.IsValid)
            {
                _dbContext.Vendors.Add(vendor);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Vendor added successfully.";
                return RedirectToAction(nameof(Index));
            }

            return View(vendor);
        }

        public IActionResult Details(int id)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            var vendor = _dbContext.Vendors.FirstOrDefault(v => v.Id == id);
            if (vendor == null)
            {
                return NotFound();
            }

            return View(vendor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            var vendor = _dbContext.Vendors.FirstOrDefault(v => v.Id == id);
            if (vendor == null)
            {
                return NotFound();
            }

            if (_dbContext.Orders.Any(o => o.VendorId == id))
            {
                TempData["ErrorMessage"] = "This vendor cannot be deleted because it is already used in orders.";
                return RedirectToAction(nameof(Index));
            }

            _dbContext.Vendors.Remove(vendor);
            _dbContext.SaveChanges();
            TempData["SuccessMessage"] = "Vendor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
