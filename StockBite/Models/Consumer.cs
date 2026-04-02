using StockBite.Models;

namespace StockBite.Models
{
    public class Consumer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public string? EmailAccessCode { get; set; }
        public string? EmailCodePurpose { get; set; }
        public DateTime? EmailCodeExpiresAt { get; set; }

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
