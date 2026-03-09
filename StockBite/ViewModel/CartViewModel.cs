namespace StockBite.ViewModels
{
    public class CartViewModel
    {
        public List<CartLineViewModel> Lines { get; set; } = new();
        public decimal Total { get; set; }
    }
}
