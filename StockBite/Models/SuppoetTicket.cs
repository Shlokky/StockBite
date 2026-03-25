using StockBite.Models;

namespace StockBite.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }
        public int? ConsumerId { get; set; }
        public Consumer? Consumer { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string AdminReply { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? RepliedAt { get; set; }
    }
}
