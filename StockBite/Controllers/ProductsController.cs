using Microsoft.AspNetCore.Mvc;
using StockBite.Data;
using StockBite.Helpers;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductsController(ApplicationDbContext dbContext, IRoleContext roleContext)
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

            return View(_dbContext.Products.ToList());
        }

        public IActionResult Create()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            ViewBag.Categories = ProductCatalogHelper.Categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            product.Name = (product.Name ?? string.Empty).Trim();
            product.Description = product.Description?.Trim();
            product.Category = ProductCatalogHelper.GetCategoryForProduct(product.Name, product.Category);
            product.ImageUrl = ProductCatalogHelper.GetImageUrl(product.ImageUrl?.Trim(), product.Category, product.Name);

            if (ModelState.IsValid)
            {
                _dbContext.Products.Add(product);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Product added successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = ProductCatalogHelper.Categories;
            return View(product);
        }

        public IActionResult Details(int id)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }

            var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (_dbContext.Orders.Any(o => o.ProductId == id))
            {
                TempData["ErrorMessage"] = "This product cannot be deleted because it is already used in orders.";
                return RedirectToAction(nameof(Index));
            }

            _dbContext.Products.Remove(product);
            _dbContext.SaveChanges();
            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
