using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockBite.Data;
using StockBite.Models;
using StockBite.Services;

namespace StockBite.Controllers
{
    public class StockManagerController : BaseController
    {
        private readonly ApplicationDbContext _db;
        private readonly OrderStatusEmailService _orderStatusEmailService;

        public StockManagerController(
            ApplicationDbContext db,
            IRoleContext roleContext,
            OrderStatusEmailService orderStatusEmailService)
            : base(roleContext)
        {
            _db = db;
            _orderStatusEmailService = orderStatusEmailService;
        }

        public IActionResult Index()
        {
            if (!IsAuthorized(UserRole.StockManager)) return RedirectToUnauthorized();
            return View(_db.Products.ToList());
        }

        public IActionResult CompareVendors(int id)
        {
            if (!IsAuthorized(UserRole.StockManager)) return RedirectToUnauthorized();

            var product = _db.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();

            ViewBag.Product = product;

            var vendorProducts = _db.VendorProducts
                .Include(x => x.Vendor)
                .Include(x => x.Product)
                .Where(x => x.ProductId == id)
                .OrderBy(x => x.Price)
                .ToList();

            return View(vendorProducts);
        }

        public IActionResult PendingOrders()
        {
            if (!IsAuthorized(UserRole.StockManager)) return RedirectToUnauthorized();

            var orders = _db.Orders
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.Consumer)
                .Where(x => x.Status != OrderStatus.Rejected)
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            if (!IsAuthorized(UserRole.StockManager)) return RedirectToUnauthorized();

            var order = _db.Orders
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.Consumer)
                .FirstOrDefault(x => x.Id == orderId);
            if (order == null) return NotFound();

            if (order.Status != OrderStatus.Pending)
            {
                TempData["ErrorMessage"] = "Only pending orders can be approved.";
                return RedirectToAction(nameof(PendingOrders));
            }

            order.Status = OrderStatus.Approved;
            order.ApprovedAt = DateTime.Now;
            _db.SaveChanges();
            await _orderStatusEmailService.SendApprovedEmailAsync(order.Consumer, order);

            TempData["SuccessMessage"] = $"Order #{order.Id} approved successfully.";
            return RedirectToAction(nameof(PendingOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDelivered(int orderId)
        {
            if (!IsAuthorized(UserRole.StockManager)) return RedirectToUnauthorized();

            var order = _db.Orders
                .Include(x => x.Product)
                .Include(x => x.Vendor)
                .Include(x => x.Consumer)
                .FirstOrDefault(x => x.Id == orderId);
            if (order == null) return NotFound();

            if (order.Status != OrderStatus.Approved)
            {
                TempData["ErrorMessage"] = "Only approved orders can be marked as delivered.";
                return RedirectToAction(nameof(PendingOrders));
            }

            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.Now;
            _db.SaveChanges();
            await _orderStatusEmailService.SendDeliveredEmailAsync(order.Consumer, order);

            TempData["SuccessMessage"] = $"Order #{order.Id} marked as delivered.";
            return RedirectToAction(nameof(PendingOrders));
        }
    }
}
