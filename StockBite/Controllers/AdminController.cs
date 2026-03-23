using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockBite.Controllers;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;
namespace StockBite.Controllers
{
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext, IRoleContext roleContext)
            : base(roleContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            return View();
        }

        public IActionResult NaturalCalamity()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }
            ViewData["ProductId"] = new SelectList(_dbContext.Products, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NaturalCalamity(int productId)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }
            var product = _dbContext.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                ViewData["ProductId"] = new SelectList(_dbContext.Products, "Id", "Name");
                ModelState.AddModelError("", "Product not found.");
                return View();
            }

            var vendorProducts = _dbContext.VendorProducts
                .Where(vp => vp.ProductId == productId)
                .ToList();

            if (vendorProducts.Count == 0)
            {
                TempData["ErrorMessage"] = "No vendor stock found for this product.";
                return RedirectToAction(nameof(Dashboard));
            }

            var cheapestVendor = vendorProducts.OrderBy(vp => vp.Price).First();

            foreach (var item in vendorProducts)
            {
                if (item.Id == cheapestVendor.Id)
                {
                    continue;
                }

                item.Price = Math.Round(item.Price * 1.5M, 2);
                item.Quantity = Math.Max(0, item.Quantity - 50);
            }

            _dbContext.SaveChanges();
            TempData["SuccessMessage"] = $"Natural calamity applied to {product.Name}.";
            return RedirectToAction(nameof(Dashboard));
        }
    }
}
