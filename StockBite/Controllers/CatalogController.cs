using Microsoft.AspNetCore.Mvc;
using StockBite.Models;

namespace StockBite.Controllers
{
    public class CatalogController : Controller
    {
        public IActionResult Index()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Rice", Price = 20, Quantity = 100 },
                new Product { Id = 2, Name = "Flour", Price = 15, Quantity = 50 },
                new Product { Id = 3, Name = "Cooking Oil", Price = 30, Quantity = 25 }
            };

            return View(products);
        }
    }
}
