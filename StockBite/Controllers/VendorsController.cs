using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Controllers;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;


namespace StockBite.Controllers
{
    public class VendorsController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRoleContext _roleContext;

        public VendorsController(ApplicationDbContext dbContext, IRoleContext roleContext)
            : base(roleContext)
        {
            _dbContext = dbContext;
            _roleContext = roleContext;
        }

        private int? ResolveVendorId()
        {
            var vendorId = CurrentVendorId;
            if (!vendorId.HasValue)
            {
                var firstVendor = _dbContext.Vendors.OrderBy(v => v.Id).FirstOrDefault();
                if (firstVendor == null)
                {
                    return null;
                }

                vendorId = firstVendor.Id;
                _roleContext.SetVendorId(vendorId.Value);
            }

            return vendorId;
        }

        public IActionResult Dashboard()
        {
            if (!IsAuthorized(UserRole.Vendor))
            {
                return RedirectToUnauthorized();
            }
            var vendorId = ResolveVendorId();
            if (!vendorId.HasValue)
            {
                TempData["ErrorMessage"] = "No vendors found in the database.";
                return RedirectToAction("Index", "Home");
            }

            var vendorProducts = _dbContext.VendorProducts
                .Include(vp => vp.Product)
                .Include(vp => vp.Vendor)
                .Where(vp => vp.VendorId == vendorId.Value)
                .ToList();

            ViewBag.VendorName = _dbContext.Vendors.FirstOrDefault(v => v.Id == vendorId.Value)?.Name;
            return View(vendorProducts);
        }

        public IActionResult EditProduct(int id)
        {
            if (!IsAuthorized(UserRole.Vendor))
            {
                return RedirectToUnauthorized();
            }
            var vendorId = ResolveVendorId();
            if (!vendorId.HasValue)
            {
                return RedirectToUnauthorized();
            }

            var vendorProduct = _dbContext.VendorProducts
                .Include(vp => vp.Product)
                .FirstOrDefault(vp => vp.Id == id && vp.VendorId == vendorId.Value);

            if (vendorProduct == null)
            {
                return NotFound();
            }

            return View(vendorProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(int id, [Bind("Id,Price,Quantity")] VendorProduct vendorProduct)
        {
            if (!IsAuthorized(UserRole.Vendor))
            {
                return RedirectToUnauthorized();
            }
            if (id != vendorProduct.Id)
            {
                return NotFound();
            }

            var vendorId = ResolveVendorId();
            if (!vendorId.HasValue)
            {
                return RedirectToUnauthorized();
            }

            var existingVendorProduct = _dbContext.VendorProducts
                .Include(vp => vp.Product)
                .FirstOrDefault(vp => vp.Id == id && vp.VendorId == vendorId.Value);

            if (existingVendorProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingVendorProduct.Price = vendorProduct.Price;
                existingVendorProduct.Quantity = vendorProduct.Quantity;
                _dbContext.SaveChanges();
                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction(nameof(Dashboard));
            }

            vendorProduct.Product = existingVendorProduct.Product;
            return View(vendorProduct);
        }

        public IActionResult Orders()
        {
            if (!IsAuthorized(UserRole.Vendor))
            {
                return RedirectToUnauthorized();
            }
            var vendorId = ResolveVendorId();
            if (!vendorId.HasValue)
            {
                return RedirectToUnauthorized();
            }

            var orders = _dbContext.Orders
                .Include(o => o.Product)
                .Include(o => o.Vendor)
                .Where(o => o.VendorId == vendorId.Value)
                .ToList();

            ViewBag.VendorName = _dbContext.Vendors.FirstOrDefault(v => v.Id == vendorId.Value)?.Name;
            return View(orders);
        }
    }
}
