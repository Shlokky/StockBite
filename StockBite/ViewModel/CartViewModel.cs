using StockBite.ViewModels;

namespace StockBite.ViewModels
{
    public class CartViewModel
    {
        public List<CartLineViewModel> Lines { get; set; } = new();
        public decimal Total { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
