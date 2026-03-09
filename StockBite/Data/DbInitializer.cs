using StockBite.Data;
using StockBite.Models;

namespace StockBite.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Products.Any() || context.Vendors.Any())
            {
                return;
            }

            var products = new List<Product>
            {
                new Product { Name = "Onions", Description = "Fresh red onions", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Onion.jpg" },
                new Product { Name = "Potatoes", Description = "Russet potatoes", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/76/Potato.jpg" },
                new Product { Name = "Tomatoes", Description = "Vine-ripened tomatoes", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/76/Tomato_2.jpg" },
                new Product { Name = "Lettuce", Description = "Crisp romaine lettuce", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6b/Romaine_lettuce.jpg" },
                new Product { Name = "Carrots", Description = "Sweet orange carrots", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Carrots.jpg" },
                new Product { Name = "Cucumbers", Description = "Cool and crunchy", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Cucumber_picture.jpg" },
                new Product { Name = "Bell Peppers", Description = "Mixed colors", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5c/Red_bell_pepper.jpg" },
                new Product { Name = "Spinach", Description = "Fresh baby spinach", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7b/Spinach_%282009%29.jpg" },
                new Product { Name = "Garlic", Description = "Aromatic garlic bulbs", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/22/Garlic.jpg" },
                new Product { Name = "Ginger", Description = "Fresh ginger root", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7f/Ginger_Root_%28Zingiber_officinale%29.jpg" },
                new Product { Name = "Broccoli", Description = "Green broccoli florets", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/41/Broccoli.jpg" },
                new Product { Name = "Cauliflower", Description = "White cauliflower heads", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/25/Cauliflower.JPG" },
                new Product { Name = "Mushrooms", Description = "Button mushrooms", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/03/Mushroom_%2812329086143%29.jpg" },
                new Product { Name = "Zucchini", Description = "Tender zucchini", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3f/Zucchini.jpg" },
                new Product { Name = "Cabbage", Description = "Green cabbage", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/70/Cabbage.jpg" }
            };

            var vendors = new List<Vendor>
            {
                new Vendor { Name = "Farm Fresh Supplies" },
                new Vendor { Name = "Green Harvest Co." },
                new Vendor { Name = "Urban Produce Distributors" },
                new Vendor { Name = "Sunrise AgriHub" },
                new Vendor { Name = "River Valley Foods" },
                new Vendor { Name = "Golden Fields Traders" },
                new Vendor { Name = "HarvestBridge Logistics" },
                new Vendor { Name = "Sprout & Root Partners" }
            };

            context.Products.AddRange(products);
            context.Vendors.AddRange(vendors);
            context.Consumers.AddRange(new List<Consumer>
            {
                new Consumer { Name = "Avery Singh" },
                new Consumer { Name = "Mia Patel" },
                new Consumer { Name = "Lucas Chen" },
                new Consumer { Name = "Sofia Ramirez" },
                new Consumer { Name = "Ethan Brooks" },
                new Consumer { Name = "Zara Khan" },
                new Consumer { Name = "Noah Wilson" },
                new Consumer { Name = "Emma Johnson" }
            });
            context.SaveChanges();

            var basePrices = new Dictionary<string, decimal>
            {
                ["Onions"] = 1.10M,
                ["Potatoes"] = 0.80M,
                ["Tomatoes"] = 1.75M,
                ["Lettuce"] = 1.20M,
                ["Carrots"] = 0.95M,
                ["Cucumbers"] = 1.05M,
                ["Bell Peppers"] = 2.20M,
                ["Spinach"] = 1.60M,
                ["Garlic"] = 2.00M,
                ["Ginger"] = 2.50M,
                ["Broccoli"] = 1.90M,
                ["Cauliflower"] = 2.10M,
                ["Mushrooms"] = 2.40M,
                ["Zucchini"] = 1.30M,
                ["Cabbage"] = 1.00M
            };

            var rng = new Random(42);
            var vendorProducts = new List<VendorProduct>();

            foreach (var vendor in vendors)
            {
                var productCount = rng.Next(7, 12);
                var selectedProducts = products
                    .OrderBy(_ => rng.Next())
                    .Take(productCount)
                    .ToList();

                foreach (var product in selectedProducts)
                {
                    var basePrice = basePrices[product.Name];
                    var multiplier = 0.85M + (decimal)rng.NextDouble() * 0.45M; // 0.85 - 1.30
                    var price = Math.Round(basePrice * multiplier, 2);
                    var quantity = rng.Next(40, 220);

                    vendorProducts.Add(new VendorProduct
                    {
                        VendorId = vendor.Id,
                        ProductId = product.Id,
                        Price = price,
                        Quantity = quantity
                    });
                }
            }

            context.VendorProducts.AddRange(vendorProducts);
            context.SaveChanges();
        }
    }
}
