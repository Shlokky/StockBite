using StockBite.Models;

namespace StockBite.Services
{
    public class ConsumerAuthCodeService
    {
        public void SetVerificationCode(Consumer consumer, string code)
        {
            consumer.EmailAccessCode = code;
            consumer.EmailCodePurpose = "verify";
            consumer.EmailCodeExpiresAt = DateTime.Now.AddMinutes(10);
        }

        public void SetLoginCode(Consumer consumer, string code)
        {
            consumer.EmailAccessCode = code;
            consumer.EmailCodePurpose = "login";
            consumer.EmailCodeExpiresAt = DateTime.Now.AddMinutes(10);
        }

        public bool IsVerificationCodeValid(Consumer consumer, string code)
        {
            return IsCodeValid(consumer, code, "verify");
        }

        public bool IsLoginCodeValid(Consumer consumer, string code)
        {
            return IsCodeValid(consumer, code, "login");
        }

        public void ClearCode(Consumer consumer)
        {
            consumer.EmailAccessCode = null;
            consumer.EmailCodePurpose = null;
            consumer.EmailCodeExpiresAt = null;
        }

        private bool IsCodeValid(Consumer consumer, string code, string purpose)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(consumer.EmailAccessCode))
            {
                return false;
            }

            if (!string.Equals(consumer.EmailCodePurpose, purpose, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!consumer.EmailCodeExpiresAt.HasValue || consumer.EmailCodeExpiresAt.Value < DateTime.Now)
            {
                return false;
            }

            return consumer.EmailAccessCode == code.Trim();
        }
    }
}
