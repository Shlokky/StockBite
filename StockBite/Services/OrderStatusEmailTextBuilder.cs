using StockBite.Models;

namespace StockBite.Services
{
    public class OrderStatusEmailTextBuilder
    {
        public string BuildPlacedSubject(Order order)
        {
            return $"StockBite order #{order.Id} placed";
        }

        public string BuildPlacedBody(Order order)
        {
            return $@"Hello {order.CustomerName},

Your order has been placed successfully.

Order number: {order.Id}
Product: {order.Product.Name}
Vendor: {order.Vendor.Name}
Quantity: {order.Quantity}
Total price: {order.TotalPrice:C}
Status: {order.Status}";
        }

        public string BuildApprovedSubject(Order order)
        {
            return $"StockBite order #{order.Id} approved";
        }

        public string BuildApprovedBody(Order order)
        {
            return $@"Hello {order.CustomerName},

Your order has been approved.

Order number: {order.Id}
Product: {order.Product.Name}
Vendor: {order.Vendor.Name}
Quantity: {order.Quantity}
Approved at: {order.ApprovedAt:yyyy-MM-dd HH:mm}
Status: {order.Status}";
        }

        public string BuildDeliveredSubject(Order order)
        {
            return $"StockBite order #{order.Id} delivered";
        }

        public string BuildDeliveredBody(Order order)
        {
            return $@"Hello {order.CustomerName},

Your order has been marked as delivered.

Order number: {order.Id}
Product: {order.Product.Name}
Vendor: {order.Vendor.Name}
Quantity: {order.Quantity}
Delivered at: {order.DeliveredAt:yyyy-MM-dd HH:mm}
Status: {order.Status}";
        }
    }
}
