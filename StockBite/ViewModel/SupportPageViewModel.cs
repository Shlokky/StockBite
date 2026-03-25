using StockBite.Models;

namespace StockBite.ViewModels
{
    public class SupportPageViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<SupportTicket> Tickets { get; set; } = new();
    }
}
