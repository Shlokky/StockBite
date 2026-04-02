using Microsoft.EntityFrameworkCore;
using StockBite.Data;
using StockBite.Helpers;
using StockBite.Models;

namespace StockBite.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            var shouldResetDatabase = Environment.GetEnvironmentVariable("RESET_STOCKBITE_DB") == "true";
            var shouldClearRuntimeData = Environment.GetEnvironmentVariable("RESET_STOCKBITE_RUNTIME_DATA") == "true";

            if (shouldResetDatabase)
            {
                context.Database.EnsureDeleted();
            }

            
            context.Database.EnsureCreated();
            EnsureOrderColumns(context);

            if (shouldClearRuntimeData)
            {
                ClearRuntimeData(context);
            }

            var productSeeds = new List<Product>
            {
                new Product { Name = "Onions", Description = "Fresh red onions", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/9/93/Onion.jpg" },
                new Product { Name = "Potatoes", Description = "Russet potatoes", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/76/Potato.jpg" },
                new Product { Name = "Tomatoes", Description = "Vine-ripened tomatoes", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/76/Tomato_2.jpg" },
                new Product { Name = "Lettuce", Description = "Crisp romaine lettuce", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6b/Romaine_lettuce.jpg" },
                new Product { Name = "Carrots", Description = "Sweet orange carrots", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Carrots.jpg" },
                new Product { Name = "Cucumbers", Description = "Cool and crunchy", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Cucumber_picture.jpg" },
                new Product { Name = "Bell Peppers", Description = "Mixed colors", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5c/Red_bell_pepper.jpg" },
                new Product { Name = "Spinach", Description = "Fresh baby spinach", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7b/Spinach_%282009%29.jpg" },
                new Product { Name = "Garlic", Description = "Aromatic garlic bulbs", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/22/Garlic.jpg" },
                new Product { Name = "Ginger", Description = "Fresh ginger root", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/7f/Ginger_Root_%28Zingiber_officinale%29.jpg" },
                new Product { Name = "Broccoli", Description = "Green broccoli florets", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/4/41/Broccoli.jpg" },
                new Product { Name = "Cauliflower", Description = "White cauliflower heads", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/25/Cauliflower.JPG" },
                new Product { Name = "Mushrooms", Description = "Button mushrooms", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/03/Mushroom_%2812329086143%29.jpg" },
                new Product { Name = "Zucchini", Description = "Tender zucchini", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/3f/Zucchini.jpg" },
                new Product { Name = "Cabbage", Description = "Green cabbage", Category = "Veggies", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/7/70/Cabbage.jpg" },
                new Product { Name = "Corn Flour", Description = "Fine yellow corn flour", Category = "Flour", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/5/5f/Cornmeal.jpg" },
                new Product { Name = "Wheat Flour", Description = "Multi-purpose wheat flour", Category = "Flour", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/00/Flour_-_03.jpg" },
                new Product { Name = "Basmati Rice", Description = "Long-grain aromatic rice", Category = "Grains", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/6/6f/Rice_grains.jpg" },
                new Product { Name = "Jasmine Rice", Description = "Fragrant jasmine rice", Category = "Grains", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0f/Jasmine_rice.jpg" },
                new Product { Name = "Sunflower Oil", Description = "Light sunflower cooking oil", Category = "Oil", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/2f/Sunflower_oil.jpg" },
                new Product { Name = "Olive Oil", Description = "Extra virgin olive oil", Category = "Oil", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c8/Olive_oil.jpg" },
                new Product { Name = "Canola Oil", Description = "Refined canola oil", Category = "Oil", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/e/e9/Canola_Oil.jpg" },
                new Product { Name = "Cheese", Description = "Processed cheese blocks", Category = "Dairy", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/8/88/Swiss_cheese.jpg" },
                new Product { Name = "Milk", Description = "Fresh whole milk", Category = "Dairy", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a4/Milk_glass.jpg" },
                new Product { Name = "Butter", Description = "Salted butter packs", Category = "Dairy", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/03/Butter.jpg" },
                new Product { Name = "Yogurt", Description = "Plain dairy yogurt", Category = "Dairy", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/e/e0/Yogurt.jpg" },
                new Product { Name = "Bread", Description = "Fresh sandwich bread loaf", Category = "Bakery", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/33/Fresh_made_bread_05.jpg" },
                new Product { Name = "Buns", Description = "Soft burger buns", Category = "Bakery", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/2b/Brioche_hamburger_buns.jpg" },
                new Product { Name = "Chicken", Description = "Boneless chicken cuts", Category = "Meat", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/2/25/Raw_Chicken.jpg" },
                new Product { Name = "Beef", Description = "Fresh beef cuts", Category = "Meat", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/3/34/Raw_beef_steak.jpg" },
                new Product { Name = "Mutton", Description = "Fresh mutton pieces", Category = "Meat", ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/0/0a/Raw_mutton.jpg" }
            };

            var vendorSeeds = new List<Vendor>
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

            var consumerNames = new[]
            {
                "Avery Singh", "Mia Patel", "Lucas Chen", "Sofia Ramirez",
                "Ethan Brooks", "Zara Khan", "Noah Wilson", "Emma Johnson"
            };

            var basePrices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
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
                ["Cabbage"] = 1.00M,
                ["Corn Flour"] = 1.60M,
                ["Wheat Flour"] = 1.30M,
                ["Basmati Rice"] = 2.40M,
                ["Jasmine Rice"] = 2.20M,
                ["Sunflower Oil"] = 4.60M,
                ["Olive Oil"] = 6.80M,
                ["Canola Oil"] = 4.20M,
                ["Cheese"] = 3.40M,
                ["Milk"] = 1.20M,
                ["Butter"] = 2.70M,
                ["Yogurt"] = 1.50M,
                ["Bread"] = 1.80M,
                ["Buns"] = 1.40M,
                ["Chicken"] = 5.60M,
                ["Beef"] = 7.20M,
                ["Mutton"] = 8.00M
            };

            UpsertProducts(context, productSeeds);
            UpsertVendors(context, vendorSeeds);
            UpsertConsumers(context, consumerNames);
            UpsertVendorProducts(context, basePrices);
        }

        private static void EnsureOrderColumns(ApplicationDbContext context)
        {
            context.Database.ExecuteSqlRaw("""
                IF COL_LENGTH('Orders', 'CustomerName') IS NULL
                BEGIN
                    ALTER TABLE Orders ADD CustomerName nvarchar(200) NOT NULL DEFAULT ''
                END
                IF COL_LENGTH('Orders', 'DeliveryAddress') IS NULL
                BEGIN
                    ALTER TABLE Orders ADD DeliveryAddress nvarchar(300) NOT NULL DEFAULT ''
                END
                IF COL_LENGTH('Orders', 'PaymentMethod') IS NULL
                BEGIN
                    ALTER TABLE Orders ADD PaymentMethod nvarchar(50) NOT NULL DEFAULT ''
                END
                IF COL_LENGTH('Orders', 'DeliveredAt') IS NULL
                BEGIN
                    ALTER TABLE Orders ADD DeliveredAt datetime2 NULL
                END
                IF COL_LENGTH('Consumers', 'GuestCode') IS NULL
                BEGIN
                    ALTER TABLE Consumers ADD GuestCode nvarchar(50) NOT NULL DEFAULT ''
                END
                IF COL_LENGTH('Products', 'Category') IS NULL
                BEGIN
                    ALTER TABLE Products ADD Category nvarchar(50) NOT NULL DEFAULT 'Veggies'
                END
                IF COL_LENGTH('Consumers', 'Email') IS NULL
                BEGIN
                    ALTER TABLE Consumers ADD Email nvarchar(200) NULL
                END
                IF COL_LENGTH('Consumers', 'EmailVerified') IS NULL
                BEGIN
                    ALTER TABLE Consumers ADD EmailVerified bit NOT NULL DEFAULT 0
                END
                IF COL_LENGTH('Consumers', 'EmailAccessCode') IS NULL
                BEGIN
                    ALTER TABLE Consumers ADD EmailAccessCode nvarchar(20) NULL
                END
                IF COL_LENGTH('Consumers', 'EmailCodePurpose') IS NULL
                BEGIN
                    ALTER TABLE Consumers ADD EmailCodePurpose nvarchar(20) NULL
                END
                IF COL_LENGTH('Consumers', 'EmailCodeExpiresAt') IS NULL
                BEGIN
                    ALTER TABLE Consumers ADD EmailCodeExpiresAt datetime2 NULL
                END
                IF OBJECT_ID('SupportTickets', 'U') IS NULL
                BEGIN
                    CREATE TABLE SupportTickets
                    (
                        Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        ConsumerId int NULL,
                        CustomerName nvarchar(200) NOT NULL DEFAULT '',
                        CustomerEmail nvarchar(200) NOT NULL DEFAULT '',
                        Subject nvarchar(200) NOT NULL DEFAULT '',
                        Message nvarchar(1000) NOT NULL DEFAULT '',
                        AdminReply nvarchar(1000) NOT NULL DEFAULT '',
                        CustomerReply nvarchar(1000) NOT NULL DEFAULT '',
                        IsResolved bit NOT NULL DEFAULT 0,
                        CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
                        RepliedAt datetime2 NULL,
                        CustomerRepliedAt datetime2 NULL
                    )
                END
                IF COL_LENGTH('SupportTickets', 'CustomerReply') IS NULL
                BEGIN
                    ALTER TABLE SupportTickets ADD CustomerReply nvarchar(1000) NOT NULL DEFAULT ''
                END
                IF COL_LENGTH('SupportTickets', 'CustomerRepliedAt') IS NULL
                BEGIN
                    ALTER TABLE SupportTickets ADD CustomerRepliedAt datetime2 NULL
                END
                IF COL_LENGTH('SupportTickets', 'ConsumerId') IS NOT NULL
                   AND NOT EXISTS
                   (
                       SELECT 1
                       FROM sys.foreign_keys
                       WHERE name = 'FK_SupportTickets_Consumers_ConsumerId'
                   )
                BEGIN
                    ALTER TABLE SupportTickets
                    ADD CONSTRAINT FK_SupportTickets_Consumers_ConsumerId
                    FOREIGN KEY (ConsumerId) REFERENCES Consumers(Id)
                END
                """);
        }

        private static void UpsertProducts(ApplicationDbContext context, List<Product> productSeeds)
        {
            var existingProducts = context.Products.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var seed in productSeeds)
            {
                if (existingProducts.TryGetValue(seed.Name, out var existing))
                {
                    if (string.IsNullOrWhiteSpace(existing.Description))
                    {
                        existing.Description = seed.Description;
                    }
                    existing.Category = ProductCatalogHelper.GetCategoryForProduct(existing.Name, existing.Category);
                    if (string.IsNullOrWhiteSpace(existing.ImageUrl))
                    {
                        existing.ImageUrl = ProductCatalogHelper.GetImageUrl(seed.ImageUrl, existing.Category, existing.Name);
                    }
                }
                else
                {
                    seed.Category = ProductCatalogHelper.GetCategoryForProduct(seed.Name, seed.Category);
                    seed.ImageUrl = ProductCatalogHelper.GetImageUrl(seed.ImageUrl, seed.Category, seed.Name);
                    context.Products.Add(seed);
                }
            }

            context.SaveChanges();
        }

        private static void UpsertVendors(ApplicationDbContext context, List<Vendor> vendorSeeds)
        {
            var existingVendorNames = context.Vendors.Select(v => v.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var vendor in vendorSeeds)
            {
                if (!existingVendorNames.Contains(vendor.Name))
                {
                    context.Vendors.Add(vendor);
                }
            }
            context.SaveChanges();
        }

        private static void UpsertConsumers(ApplicationDbContext context, IEnumerable<string> consumerNames)
        {
            var existingConsumers = context.Consumers.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var consumerName in consumerNames)
            {
                if (!existingConsumers.Contains(consumerName))
                {
                    context.Consumers.Add(new Consumer { Name = consumerName });
                }
            }
            context.SaveChanges();
        }

        private static void UpsertVendorProducts(ApplicationDbContext context, IDictionary<string, decimal> basePrices)
        {
            var products = context.Products.AsNoTracking().OrderBy(p => p.Name).ToList();
            var vendors = context.Vendors.AsNoTracking().OrderBy(v => v.Name).ToList();

            if (!products.Any() || !vendors.Any())
            {
                return;
            }

            var rng = new Random(42);
            var vendorProducts = context.VendorProducts.ToList();
            var existingVendorProducts = vendorProducts.ToDictionary(
                vp => (vp.VendorId, vp.ProductId));
            var vendorProductsToAdd = new List<VendorProduct>();

            foreach (var vendor in vendors)
            {
                var productCount = rng.Next(10, 16);
                var selectedProducts = products
                    .OrderBy(_ => rng.Next())
                    .Take(productCount)
                    .ToList();

                foreach (var product in selectedProducts)
                {
                    var basePrice = basePrices.TryGetValue(product.Name, out var value) ? value : 2.00M;
                    var multiplier = 0.88M + (decimal)rng.NextDouble() * 0.35M;
                    var price = Math.Round(basePrice * multiplier, 2);
                    var quantity = rng.Next(450, 901);

                    if (existingVendorProducts.TryGetValue((vendor.Id, product.Id), out var existing))
                    {
                        existing.Price = price;
                        existing.Quantity = quantity;
                        continue;
                    }

                    vendorProductsToAdd.Add(new VendorProduct
                    {
                        VendorId = vendor.Id,
                        ProductId = product.Id,
                        Price = price,
                        Quantity = quantity
                    });
                }
            }

            if (vendorProductsToAdd.Any())
            {
                context.VendorProducts.AddRange(vendorProductsToAdd);
            }

            context.SaveChanges();
        }

        private static void ClearRuntimeData(ApplicationDbContext context)
        {
            if (context.Orders.Any())
            {
                context.Orders.RemoveRange(context.Orders);
            }

            if (context.SupportTickets.Any())
            {
                context.SupportTickets.RemoveRange(context.SupportTickets);
            }

            var guestConsumers = context.Consumers
                .Where(c => !string.IsNullOrWhiteSpace(c.GuestCode))
                .ToList();

            if (guestConsumers.Any())
            {
                context.Consumers.RemoveRange(guestConsumers);
            }

            context.SaveChanges();
        }
    }
}
