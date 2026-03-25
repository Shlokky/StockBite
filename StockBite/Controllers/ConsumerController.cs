using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Controllers;
using StockBite.Data;
using StockBite.Helpers;
using StockBite.Models;
using StockBite.Services;
using StockBite.ViewModels;


namespace StockBitePrototype.Controllers
{
    public class ConsumerController : BaseController
    {
        private readonly ApplicationDbContext _dbContext;
        private const string CartSessionKey = "ConsumerCart";
        private static readonly List<string> Categories =
        [
            "All",
            "Veggies",
            "Dairy",
            "Meat",
            "Flour",
            "Oil",
            "Bakery",
            "Grains"
        ];

        public ConsumerController(ApplicationDbContext dbContext, IRoleContext roleContext)
            : base(roleContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index(string? category)
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var selectedCategory = GetSelectedCategory(category);
            var products = GetProductsByCategory(selectedCategory);
            var consumerId = CurrentConsumerId;
            var consumerName = GetConsumerName(consumerId);
            var consumerCode = GetConsumerCode(consumerId);
            var currentStock = GetCurrentStock(consumerId);
            var model = new ConsumerDashboardViewModel
            {
                ConsumerName = consumerName,
                ConsumerCode = consumerCode,
                Products = products,
                RecommendedProducts = GetRecommendedProducts(products, consumerId, consumerName),
                CurrentStock = currentStock,
                PriorityProducts = currentStock.Where(x => x.IsPriorityOrder).Take(4).ToList()
            };

            ViewBag.Categories = Categories;
            ViewBag.SelectedCategory = selectedCategory;

            return View(model);
        }

        public IActionResult AddToCart(int id)
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var product = _dbContext.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            var vendorProducts = _dbContext.VendorProducts
                .Include(vp => vp.Vendor)
                .Where(vp => vp.ProductId == id)
                .OrderBy(vp => vp.Price)
                .ToList();

            ViewBag.Product = product;
            return View(vendorProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int vendorProductId, int orderQuantity)
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var consumerId = CurrentConsumerId;
            if (!consumerId.HasValue)
            {
                TempData["ErrorMessage"] = "Select a consumer before placing an order.";
                return RedirectToAction("Index", "Home");
            }

            var vendorProduct = GetVendorProduct(vendorProductId);

            if (vendorProduct == null || orderQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Invalid order request.";
                return RedirectToAction(nameof(Index));
            }

            if (vendorProduct.Quantity < orderQuantity)
            {
                TempData["ErrorMessage"] = "Not enough stock available for this vendor.";
                return RedirectToAction(nameof(AddToCart), new { id = vendorProduct.ProductId });
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.VendorProductId == vendorProductId);
            var totalQuantity = orderQuantity + (existingItem?.Quantity ?? 0);

            if (totalQuantity > vendorProduct.Quantity)
            {
                TempData["ErrorMessage"] = "Requested quantity exceeds available stock.";
                return RedirectToAction(nameof(AddToCart), new { id = vendorProduct.ProductId });
            }

            if (existingItem == null)
            {
                cart.Add(new CartItem { VendorProductId = vendorProductId, Quantity = orderQuantity });
            }
            else
            {
                existingItem.Quantity = totalQuantity;
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = $"{vendorProduct.Product.Name} added to cart.";
            return RedirectToAction(nameof(Cart));
        }

        public IActionResult Cart()
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var consumerId = CurrentConsumerId;
            if (!consumerId.HasValue)
            {
                TempData["ErrorMessage"] = "Select a consumer first.";
                return RedirectToAction("Index", "Home");
            }

            return View(BuildCartViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int vendorProductId, int quantity)
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.VendorProductId == vendorProductId);
            if (item == null)
            {
                return RedirectToAction(nameof(Cart));
            }

            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                var vendorProduct = _dbContext.VendorProducts.FirstOrDefault(vp => vp.Id == vendorProductId);
                if (vendorProduct == null || quantity > vendorProduct.Quantity)
                {
                    TempData["ErrorMessage"] = "Requested quantity exceeds available stock.";
                    return RedirectToAction(nameof(Cart));
                }
                item.Quantity = quantity;
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = "Cart updated.";
            return RedirectToAction(nameof(Cart));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string customerName, string deliveryAddress, string paymentMethod)
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var consumerId = CurrentConsumerId;
            if (!consumerId.HasValue)
            {
                TempData["ErrorMessage"] = "Select a consumer first.";
                return RedirectToAction("Index", "Home");
            }

            customerName = (customerName ?? "").Trim();
            deliveryAddress = (deliveryAddress ?? "").Trim();
            paymentMethod = (paymentMethod ?? "").Trim();

            if (string.IsNullOrWhiteSpace(customerName) ||
                string.IsNullOrWhiteSpace(deliveryAddress) ||
                string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["ErrorMessage"] = "Fill in name, delivery address, and payment method.";
                return View("Cart", BuildCartViewModel(customerName, deliveryAddress, paymentMethod));
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            var vendorProducts = GetCartVendorProducts(cart);

            foreach (var item in cart)
            {
                var vp = vendorProducts.FirstOrDefault(v => v.Id == item.VendorProductId);
                if (vp == null || vp.Quantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = "Some items are no longer available in the requested quantity.";
                    return RedirectToAction(nameof(Cart));
                }
            }

            var orders = new List<Order>();

            foreach (var item in cart)
            {
                var vp = vendorProducts.First(v => v.Id == item.VendorProductId);
                orders.Add(new Order
                {
                    ConsumerId = consumerId.Value,
                    ProductId = vp.ProductId,
                    VendorId = vp.VendorId,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * vp.Price,
                    CustomerName = customerName,
                    DeliveryAddress = deliveryAddress,
                    PaymentMethod = paymentMethod,
                    Status = OrderStatus.Pending
                });
            }

            foreach (var item in cart)
            {
                var vp = vendorProducts.First(v => v.Id == item.VendorProductId);
                vp.Quantity -= item.Quantity;
            }

            _dbContext.Orders.AddRange(orders);
            _dbContext.SaveChanges();

            SaveCart(new List<CartItem>());
            TempData["SuccessMessage"] = "Order submitted successfully.";
            TempData["GuestCode"] = GetConsumerCode(consumerId);
            return RedirectToAction(nameof(Orders));
        }

        public IActionResult Orders()
        {
            if (!IsAuthorized(UserRole.Consumer))
            {
                return RedirectToUnauthorized();
            }

            var consumerId = CurrentConsumerId;
            if (!consumerId.HasValue)
            {
                TempData["ErrorMessage"] = "Select a consumer first.";
                return RedirectToAction("Index", "Home");
            }

            var orders = _dbContext.Orders
                .Include(o => o.Product)
                .Include(o => o.Vendor)
                .Where(o => o.ConsumerId == consumerId.Value)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewBag.ConsumerCode = GetConsumerCode(consumerId);
            return View(orders);
        }

        private string GetSelectedCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category) || !Categories.Contains(category))
            {
                return "All";
            }

            return category;
        }

        private List<Product> GetProductsByCategory(string selectedCategory)
        {
            var products = _dbContext.Products.OrderBy(p => p.Name).ToList();

            if (selectedCategory == "All")
            {
                return products;
            }

            return products.Where(p => GetCategoryForProduct(p.Name) == selectedCategory).ToList();
        }

        private List<Product> GetRecommendedProducts(List<Product> products, int? consumerId, string? consumerName)
        {
            if (!consumerId.HasValue)
            {
                return products.Take(4).ToList();
            }

            var orderedProductIds = _dbContext.Orders
                .Where(o => o.ConsumerId == consumerId.Value)
                .GroupBy(o => o.ProductId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToList();

            var recommended = products
                .Where(p => orderedProductIds.Contains(p.Id))
                .OrderBy(p => orderedProductIds.IndexOf(p.Id))
                .Take(4)
                .ToList();

            if (recommended.Any())
            {
                return recommended;
            }

            var preferredNames = GetDefaultRecommendedNames(consumerName);
            var defaultRecommended = products.Where(p => preferredNames.Contains(p.Name)).Take(4).ToList();

            if (defaultRecommended.Any())
            {
                return defaultRecommended;
            }

            return products.Take(4).ToList();
        }

        private string GetConsumerName(int? consumerId)
        {
            if (!consumerId.HasValue)
            {
                return "";
            }

            return _dbContext.Consumers
                .Where(c => c.Id == consumerId.Value)
                .Select(c => c.Name)
                .FirstOrDefault() ?? "";
        }

        private string GetConsumerCode(int? consumerId)
        {
            if (!consumerId.HasValue)
            {
                return "";
            }

            return _dbContext.Consumers
                .Where(c => c.Id == consumerId.Value)
                .Select(c => c.GuestCode)
                .FirstOrDefault() ?? "";
        }

        private static List<string> GetDefaultRecommendedNames(string? consumerName)
        {
            var name = consumerName?.Trim().ToLowerInvariant() ?? "";

            return name switch
            {
                var x when x.Contains("avery") || x.Contains("mia") => new List<string> { "Milk", "Cheese", "Bread", "Butter" },
                var x when x.Contains("lucas") || x.Contains("ethan") => new List<string> { "Chicken", "Beef", "Onions", "Potatoes" },
                var x when x.Contains("sofia") || x.Contains("emma") => new List<string> { "Tomatoes", "Lettuce", "Cucumbers", "Bell Peppers" },
                var x when x.Contains("zara") || x.Contains("noah") => new List<string> { "Jasmine Rice", "Wheat Flour", "Canola Oil", "Yogurt" },
                _ => new List<string> { "Onions", "Milk", "Bread", "Jasmine Rice" }
            };
        }

        private List<ConsumerStockItemViewModel> GetCurrentStock(int? consumerId)
        {
            if (!consumerId.HasValue)
            {
                return new List<ConsumerStockItemViewModel>();
            }

            return _dbContext.Orders
                .Include(o => o.Product)
                .Where(o => o.ConsumerId == consumerId.Value && o.Status == OrderStatus.Approved)
                .GroupBy(o => o.Product.Name)
                .AsEnumerable()
                .Select(g =>
                {
                    var totalQuantity = g.Sum(x => x.Quantity);
                    var lastApprovedDate = g.Max(x => x.ApprovedAt ?? x.OrderDate);
                    var daysPassed = Math.Max(0, (DateTime.Now.Date - lastApprovedDate.Date).Days);
                    var shelfLife = ProductDisplayHelper.GetShelfLifeDays(g.Key);
                    var daysLeft = shelfLife - daysPassed;
                    var usedQuantity = daysPassed * ProductDisplayHelper.GetDailyUsage(g.Key);
                    var remainingQuantity = Math.Max(0, totalQuantity - usedQuantity);
                    var isExpiringSoon = daysLeft <= 3;
                    var isPriorityOrder = remainingQuantity <= 3 || isExpiringSoon;

                    return new ConsumerStockItemViewModel
                    {
                        ProductName = g.Key,
                        Quantity = totalQuantity,
                        RemainingQuantity = remainingQuantity,
                        UnitLabel = ProductDisplayHelper.GetUnit(g.Key),
                        ExpiryText = daysLeft <= 0 ? "Expiring now" : $"Expires in {daysLeft} day(s)",
                        IsExpiringSoon = isExpiringSoon,
                        IsPriorityOrder = isPriorityOrder
                    };
                })
                .OrderByDescending(x => x.IsPriorityOrder)
                .ThenBy(x => x.RemainingQuantity)
                .Take(6)
                .ToList();
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        private VendorProduct? GetVendorProduct(int vendorProductId)
        {
            return _dbContext.VendorProducts
                .Include(vp => vp.Product)
                .Include(vp => vp.Vendor)
                .FirstOrDefault(vp => vp.Id == vendorProductId);
        }

        private List<VendorProduct> GetCartVendorProducts(List<CartItem> cart)
        {
            var vendorProductIds = cart.Select(c => c.VendorProductId).ToList();

            return _dbContext.VendorProducts
                .Include(vp => vp.Product)
                .Include(vp => vp.Vendor)
                .Where(vp => vendorProductIds.Contains(vp.Id))
                .ToList();
        }

        private CartViewModel BuildCartViewModel(string? customerName = null, string? deliveryAddress = null, string? paymentMethod = null)
        {
            var cart = GetCart();
            var vendorProducts = GetCartVendorProducts(cart);
            var lines = new List<CartLineViewModel>();

            foreach (var item in cart)
            {
                var vendorProduct = vendorProducts.FirstOrDefault(vp => vp.Id == item.VendorProductId);
                if (vendorProduct == null)
                {
                    continue;
                }

                lines.Add(new CartLineViewModel
                {
                    VendorProductId = item.VendorProductId,
                    ProductName = vendorProduct.Product.Name,
                    VendorName = vendorProduct.Vendor.Name,
                    UnitPrice = vendorProduct.Price,
                    Quantity = item.Quantity,
                    UnitLabel = ProductDisplayHelper.GetUnit(vendorProduct.Product.Name),
                    LineTotal = item.Quantity * vendorProduct.Price
                });
            }

            return new CartViewModel
            {
                Lines = lines,
                Total = lines.Sum(l => l.LineTotal),
                CustomerName = string.IsNullOrWhiteSpace(customerName) ? GetConsumerName(CurrentConsumerId) : customerName,
                DeliveryAddress = deliveryAddress ?? "",
                PaymentMethod = paymentMethod ?? ""
            };
        }

        private static string GetCategoryForProduct(string? productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
            {
                return "Veggies";
            }

            return productName.Trim().ToLowerInvariant() switch
            {
                "milk" or "cheese" or "butter" or "yogurt" => "Dairy",
                "chicken" or "beef" or "mutton" => "Meat",
                "corn flour" or "wheat flour" => "Flour",
                "sunflower oil" or "olive oil" or "canola oil" => "Oil",
                "bread" or "buns" => "Bakery",
                "basmati rice" or "jasmine rice" => "Grains",
                _ => "Veggies"
            };
        }
    }
}
