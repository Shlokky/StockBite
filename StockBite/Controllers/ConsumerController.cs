using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Data;
using StockBite.Helpers;
using StockBite.Models;
using StockBite.Services;
using StockBite.ViewModels;
using StockBitePrototype.Models;
using StockBitePrototype.Services;
using System.Linq;
using System.Security.Cryptography;

namespace StockBite.Controllers
{
    public class ConsumerController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private const string CartSessionKey = "ConsumerCart";

        private static readonly List<string> Categories =
        [
            "All", "Veggies", "Dairy", "Meat", "Flour", "Oil", "Bakery", "Grains"
        ];

        public ConsumerController(ApplicationDbContext db, IRoleContext roleContext) :
base(roleContext)
        {
            _db = db;
        }

        public IActionResult Index(string? category)
        {
            var check = CheckConsumer();
            if (check != null) return check;

            var selected = string.IsNullOrWhiteSpace(category) || !Categories.Contains(category) ?
"All" : category;
            var consumerId = CurrentConsumerId;
            var products = GetProducts(selected);
            var name = GetConsumerName(consumerId);
            var stock = GetCurrentStock(consumerId);

            ViewBag.Categories = Categories;
            ViewBag.SelectedCategory = selected;

            return View(new ConsumerDashboardViewModel
            {
                ConsumerName = name,
                ConsumerCode = GetConsumerCode(consumerId),
                Products = products,
                RecommendedProducts = GetRecommendedProducts(products, consumerId, name),
                CurrentStock = stock,
                PriorityProducts = stock.Where(x => x.IsPriorityOrder).Take(4).ToList()
            });
        }

        public IActionResult AddToCart(int id)
        {
            var check = CheckConsumer();
            if (check != null) return check;

            var product = _db.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();

            ViewBag.Product = product;

            var vendorProducts = _db.VendorProducts
                .Include(x => x.Vendor)
                .Where(x => x.ProductId == id)
                .OrderBy(x => x.Price)
                .ToList();

            return View(vendorProducts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int vendorProductId, int orderQuantity)
        {
            var check = CheckConsumerWithId();
            if (check != null) return check;

            var vp = GetVendorProduct(vendorProductId);
            if (vp == null || orderQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Invalid order request.";
                return RedirectToAction(nameof(Index));
            }

            if (vp.Quantity < orderQuantity)
            {
                TempData["ErrorMessage"] = "Not enough stock available.";
                return RedirectToAction(nameof(AddToCart), new { id = vp.ProductId });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.VendorProductId == vendorProductId);
            var total = orderQuantity + (item?.Quantity ?? 0);

            if (total > vp.Quantity)
            {
                TempData["ErrorMessage"] = "Requested quantity exceeds available stock.";
                return RedirectToAction(nameof(AddToCart), new { id = vp.ProductId });
            }

            if (item == null)
                cart.Add(new CartItem
                {
                    VendorProductId = vendorProductId,
                    Quantity =
orderQuantity
                });
            else
                item.Quantity = total;

            SaveCart(cart);
            TempData["SuccessMessage"] = $"{vp.Product.Name} added to cart.";
            return RedirectToAction(nameof(Cart));
        }

        public IActionResult Cart()
        {
            var check = CheckConsumerWithId();
            if (check != null) return check;

            return View(BuildCartViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int vendorProductId, int quantity)
        {
            var check = CheckConsumer();
            if (check != null) return check;

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.VendorProductId == vendorProductId);
            if (item == null) return RedirectToAction(nameof(Cart));

            if (quantity <= 0)
            {
                cart.Remove(item);
            }
            else
            {
                var vp = _db.VendorProducts.FirstOrDefault(x => x.Id == vendorProductId);
                if (vp == null || quantity > vp.Quantity)
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
        public IActionResult Checkout(string customerName, string deliveryAddress, string
paymentMethod)
        {
            var check = CheckConsumerWithId();
            if (check != null) return check;

            customerName = Clean(customerName);
            deliveryAddress = Clean(deliveryAddress);
            paymentMethod = Clean(paymentMethod);

            if (customerName == "" || deliveryAddress == "" || paymentMethod == "")
            {
                TempData["ErrorMessage"] = "Fill all details first.";
                return View("Cart", BuildCartViewModel(customerName, deliveryAddress,
paymentMethod));
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Cart));
            }

            var consumerId = CurrentConsumerId!.Value;
            var vendorProducts = GetCartVendorProducts(cart).ToDictionary(x => x.Id);
            var orders = new List<Order>();

            foreach (var item in cart)
            {
                if (!vendorProducts.TryGetValue(item.VendorProductId, out var vp) || vp.Quantity <
item.Quantity)
                {
                    TempData["ErrorMessage"] = "Some items are not available now.";
                    return RedirectToAction(nameof(Cart));
                }

                orders.Add(new Order
                {
                    ConsumerId = consumerId,
                    ProductId = vp.ProductId,
                    VendorId = vp.VendorId,
                    Quantity = item.Quantity,
                    TotalPrice = item.Quantity * vp.Price,
                    CustomerName = customerName,
                    DeliveryAddress = deliveryAddress,
                    PaymentMethod = paymentMethod,
                    Status = OrderStatus.Pending
                });

                vp.Quantity -= item.Quantity;
            }

            _db.Orders.AddRange(orders);
            _db.SaveChanges();

            SaveCart(new List<CartItem>());
            TempData["SuccessMessage"] = "Order submitted successfully.";
            TempData["GuestCode"] = GetConsumerCode(consumerId);

            return RedirectToAction(nameof(Orders));
        }

        public IActionResult Orders()
        {
            var check = CheckConsumerWithId();
            if (check != null) return check;

            var consumerId = CurrentConsumerId!.Value;
            ViewBag.ConsumerCode = GetConsumerCode(consumerId);

            var orders = _db.Orders
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Where(x => x.ConsumerId == consumerId)
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            return View(orders);
        }

        private IActionResult? CheckConsumer()
        {
            return IsAuthorized(UserRole.Consumer) ? null : RedirectToUnauthorized();
        }

        private IActionResult? CheckConsumerWithId()
        {
            if (!IsAuthorized(UserRole.Consumer)) return RedirectToUnauthorized();

            if (!CurrentConsumerId.HasValue)
            {
                TempData["ErrorMessage"] = "Select a consumer first.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        private static string Clean(string? text) => (text ?? "").Trim();

        private List<Product> GetProducts(string category)
        {
            var products = _db.Products.OrderBy(x => x.Name).ToList();
            return category == "All" ? products : products.Where(x => GetCategory(x.Name) ==
category).ToList();
        }

        private List<Product> GetRecommendedProducts(List<Product> products, int? consumerId,
string? consumerName)
        {
            if (!consumerId.HasValue) return products.Take(4).ToList();

            var orderedIds = _db.Orders
                .Where(x => x.ConsumerId == consumerId.Value)
                .GroupBy(x => x.ProductId)
                .OrderByDescending(x => x.Count())
                .Select(x => x.Key)
                .ToList();

            var recommended = products
                .Where(x => orderedIds.Contains(x.Id))
                .OrderBy(x => orderedIds.IndexOf(x.Id))
                .Take(4)
                .ToList();

            if (recommended.Any()) return recommended;

            var names = GetDefaultNames(consumerName);
            recommended = products.Where(x => names.Contains(x.Name)).Take(4).ToList();

            return recommended.Any() ? recommended : products.Take(4).ToList();
        }

        private string GetConsumerName(int? consumerId)
        {
            if (!consumerId.HasValue) return "";

            return _db.Consumers
                .Where(x => x.Id == consumerId.Value)
                .Select(x => x.Name)
                .FirstOrDefault() ?? "";
        }

        private string GetConsumerCode(int? consumerId)
        {
            if (!consumerId.HasValue) return "";

            return _db.Consumers
                .Where(x => x.Id == consumerId.Value)
                .Select(x => x.GuestCode)
                .FirstOrDefault() ?? "";
        }

        private static List<string> GetDefaultNames(string? consumerName)
        {
            var name = Clean(consumerName).ToLower();

            if (name.Contains("avery") || name.Contains("mia")) return new List<string> { "Milk",
  "Cheese", "Bread", "Butter" };
            if (name.Contains("lucas") || name.Contains("ethan")) return new List<string>
  { "Chicken", "Beef", "Onions", "Potatoes" };
            if (name.Contains("sofia") || name.Contains("emma")) return new List<string>
  { "Tomatoes", "Lettuce", "Cucumbers", "Bell Peppers" };
            if (name.Contains("zara") || name.Contains("noah")) return new List<string> { "Jasmine
  Rice", "Wheat Flour", "Canola Oil", "Yogurt" };

              return new List<string> { "Onions", "Milk", "Bread", "Jasmine Rice" };
        }

        private List<ConsumerStockItemViewModel> GetCurrentStock(int? consumerId)
        {
            if (!consumerId.HasValue) return new List<ConsumerStockItemViewModel>();

            return _db.Orders
                .Include(x => x.Product)
                .Where(x => x.ConsumerId == consumerId.Value && x.Status == OrderStatus.Approved)
                .GroupBy(x => x.Product.Name)
                .AsEnumerable()
                .Select(x =>
                {
                    var qty = x.Sum(a => a.Quantity);
                    var lastDate = x.Max(a => a.ApprovedAt ?? a.OrderDate);
                    var days = Math.Max(0, (DateTime.Now.Date - lastDate.Date).Days);
                    var shelf = ProductDisplayHelper.GetShelfLifeDays(x.Key);
                    var leftDays = shelf - days;
                    var used = days * ProductDisplayHelper.GetDailyUsage(x.Key);
                    var leftQty = Math.Max(0, qty - used);
                    var expiring = leftDays <= 3;

                    return new ConsumerStockItemViewModel
                    {
                        ProductName = x.Key,
                        Quantity = qty,
                        RemainingQuantity = leftQty,
                        UnitLabel = ProductDisplayHelper.GetUnit(x.Key),
                        ExpiryText = leftDays <= 0 ? "Expiring now" : $"Expires in {leftDays}

day(s)",

                        IsExpiringSoon = expiring,
                        IsPriorityOrder = leftQty <= 3 || expiring
                    };
                })
                .OrderByDescending(x => x.IsPriorityOrder)
                .ThenBy(x => x.RemainingQuantity)
                .Take(6)
                .ToList();
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItem>>(CartSessionKey) ?? new
List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        private VendorProduct? GetVendorProduct(int vendorProductId)
        {
            return _db.VendorProducts
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .FirstOrDefault(x => x.Id == vendorProductId);
        }

        private List<VendorProduct> GetCartVendorProducts(List<CartItem> cart)
        {
            var ids = cart.Select(x => x.VendorProductId).ToList();

            return _db.VendorProducts
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Where(x => ids.Contains(x.Id))
                .ToList();
        }

        private CartViewModel BuildCartViewModel(string? customerName = null, string?
deliveryAddress = null, string? paymentMethod = null)
        {
            var cart = GetCart();
            var vendorProducts = GetCartVendorProducts(cart);
            var lines = new List<CartLineViewModel>();

            foreach (var item in cart)
            {
                var vp = vendorProducts.FirstOrDefault(x => x.Id == item.VendorProductId);
                if (vp == null) continue;

                lines.Add(new CartLineViewModel
                {
                    VendorProductId = item.VendorProductId,
                    ProductName = vp.Product.Name,
                    VendorName = vp.Vendor.Name,
                    UnitPrice = vp.Price,
                    Quantity = item.Quantity,
                    UnitLabel = ProductDisplayHelper.GetUnit(vp.Product.Name),
                    LineTotal = item.Quantity * vp.Price
                });
            }

            return new CartViewModel
            {
                Lines = lines,
                Total = lines.Sum(x => x.LineTotal),
                CustomerName = customerName == null || customerName == "" ?
GetConsumerName(CurrentConsumerId) : customerName,
                DeliveryAddress = deliveryAddress ?? "",
                PaymentMethod = paymentMethod ?? ""
            };
        }

        private static string GetCategory(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "Veggies";

            name = name.Trim().ToLower();

            if (name == "milk" || name == "cheese" || name == "butter" || name == "yogurt") return
"Dairy";
            if (name == "chicken" || name == "beef" || name == "mutton") return "Meat";
            if (name == "corn flour" || name == "wheat flour") return "Flour";
            if (name == "sunflower oil" || name == "olive oil" || name == "canola oil") return
"Oil";
            if (name == "bread" || name == "buns") return "Bakery";
            if (name == "basmati rice" || name == "jasmine rice") return "Grains";

            return "Veggies";
        }
    }
}
