using StockBite.Models;

namespace StockBite.Services
{
    public class OrderStatusEmailService
    {
        private readonly IEmailService _emailService;
        private readonly OrderStatusEmailTextBuilder _textBuilder;

        public OrderStatusEmailService(IEmailService emailService, OrderStatusEmailTextBuilder textBuilder)
        {
            _emailService = emailService;
            _textBuilder = textBuilder;
        }

        public async Task SendPlacedEmailAsync(Consumer consumer, Order order)
        {
            var subject = _textBuilder.BuildPlacedSubject(order);
            var body = _textBuilder.BuildPlacedBody(order);
            await SendIfPossibleAsync(consumer, subject, body);
        }

        public async Task SendApprovedEmailAsync(Consumer consumer, Order order)
        {
            var subject = _textBuilder.BuildApprovedSubject(order);
            var body = _textBuilder.BuildApprovedBody(order);
            await SendIfPossibleAsync(consumer, subject, body);
        }

        public async Task SendDeliveredEmailAsync(Consumer consumer, Order order)
        {
            var subject = _textBuilder.BuildDeliveredSubject(order);
            var body = _textBuilder.BuildDeliveredBody(order);
            await SendIfPossibleAsync(consumer, subject, body);
        }

        private async Task SendIfPossibleAsync(Consumer consumer, string subject, string body)
        {
            if (!consumer.EmailVerified)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(consumer.Email))
            {
                return;
            }

            await _emailService.SendAsync(consumer.Email, subject, body);
        }
    }
}
