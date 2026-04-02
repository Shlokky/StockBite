using StockBite.Models;

namespace StockBite.Services
{
    public class ConsumerEmailFlowService
    {
        private readonly IEmailService _emailService;

        public ConsumerEmailFlowService(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public string GenerateAccessCode()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        public async Task SendVerificationCodeAsync(Consumer consumer, string code)
        {
            if (string.IsNullOrWhiteSpace(consumer.Email))
            {
                return;
            }

            var body = $@"Hello {consumer.Name},

Your StockBite verification code is: {code}

Enter this code on the website to verify your email account.

If you did not request this, you can ignore this email.";

            await _emailService.SendAsync(consumer.Email, "StockBite email verification", body);
        }

        public async Task SendLoginCodeAsync(Consumer consumer, string code)
        {
            if (string.IsNullOrWhiteSpace(consumer.Email))
            {
                return;
            }

            var body = $@"Hello {consumer.Name},

Your StockBite login code is: {code}

Enter this code on the website to sign in.

If you did not request this, you can ignore this email.";

            await _emailService.SendAsync(consumer.Email, "StockBite login code", body);
        }

        public async Task SendOrderPlacedEmailAsync(Consumer consumer, Order order)
        {
            await SendOrderEmailAsync(
                consumer,
                $"StockBite order #{order.Id} placed",
                $@"Hello {order.CustomerName},

Your order has been placed successfully.

Order number: {order.Id}
Product: {order.Product.Name}
Vendor: {order.Vendor.Name}
Quantity: {order.Quantity}
Total price: {order.TotalPrice:C}
Status: {order.Status}");
        }

        public async Task SendOrderApprovedEmailAsync(Consumer consumer, Order order)
        {
            await SendOrderEmailAsync(
                consumer,
                $"StockBite order #{order.Id} approved",
                $@"Hello {order.CustomerName},

Your order has been approved.

Order number: {order.Id}
Product: {order.Product.Name}
Vendor: {order.Vendor.Name}
Quantity: {order.Quantity}
Approved at: {order.ApprovedAt:yyyy-MM-dd HH:mm}
Status: {order.Status}");
        }

        public async Task SendOrderDeliveredEmailAsync(Consumer consumer, Order order)
        {
            await SendOrderEmailAsync(
                consumer,
                $"StockBite order #{order.Id} delivered",
                $@"Hello {order.CustomerName},

Your order has been marked as delivered.

Order number: {order.Id}
Product: {order.Product.Name}
Vendor: {order.Vendor.Name}
Quantity: {order.Quantity}
Delivered at: {order.DeliveredAt:yyyy-MM-dd HH:mm}
Status: {order.Status}");
        }

        private async Task SendOrderEmailAsync(Consumer consumer, string subject, string body)
        {
            if (!consumer.EmailVerified || string.IsNullOrWhiteSpace(consumer.Email))
            {
                return;
            }

            await _emailService.SendAsync(consumer.Email, subject, body);
        }
    }
}
