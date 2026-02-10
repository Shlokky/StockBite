using StockBite.Models;

namespace StockBite.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Products.Any())
                return;

            context.Products.AddRange(
                new Product { Name = "Onions (50 lb bag)", Price = 0, Quantity = 0 },
                new Product { Name = "Rice (25 kg)", Price = 0, Quantity = 0 }
            );

            context.SaveChanges();
        }
    }
}
