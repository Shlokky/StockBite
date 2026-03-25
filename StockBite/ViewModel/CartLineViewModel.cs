namespace StockBite.ViewModels
{
    public class CartLineViewModel
    {
        public int VendorProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VendorName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string UnitLabel { get; set; } = string.Empty;
        public decimal LineTotal { get; set; }
    }
}
