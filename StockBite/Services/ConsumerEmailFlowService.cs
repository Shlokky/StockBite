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

    }
}
