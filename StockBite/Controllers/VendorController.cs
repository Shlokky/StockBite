using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Data;
using StockBite.Models;

namespace StockBite.Controllers
{
    public class VendorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Compare(int productId)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            var vendors = await _context.VendorProducts
                .Include(vp => vp.Vendor)
                .Where(vp => vp.ProductId == productId)
                .ToListAsync();

            var vm = new VendorComparisonVM
            {
                ProductName = product.Name,
                VendorOptions = vendors
            };

            return View(vm);
        }
    }
}
