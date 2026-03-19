using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockBite.Controllers;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;
namespace StockBite.Controllers
{
    public class VendorProductsController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;

        public VendorProductsController(ApplicationDbContext dbContext, IRoleContext roleContext)
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
            var vendorProducts = _dbContext.VendorProducts
                                           .Include(vp => vp.Product)
                                           .Include(vp => vp.Vendor)
                                           .ToList();
            return View(vendorProducts);
        }

        public IActionResult Create()
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }
            ViewData["ProductId"] = new SelectList(_dbContext.Products, "Id", "Name");
            ViewData["VendorId"] = new SelectList(_dbContext.Vendors, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("VendorId,ProductId,Price,Quantity")] VendorProduct vendorProduct)
        {
            if (!IsAuthorized(UserRole.Admin))
            {
                return RedirectToUnauthorized();
            }
            if (ModelState.IsValid)
            {
                _dbContext.VendorProducts.Add(vendorProduct);
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Vendor product added successfully.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_dbContext.Products, "Id", "Name", vendorProduct.ProductId);
            ViewData["VendorId"] = new SelectList(_dbContext.Vendors, "Id", "Name", vendorProduct.VendorId);
            return View(vendorProduct);
        }
    }
}
