namespace StockBite.ViewModels
{
    public class ConsumerStockItemViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int RemainingQuantity { get; set; }
        public string UnitLabel { get; set; } = string.Empty;
        public string ExpiryText { get; set; } = string.Empty;
        public bool IsExpiringSoon { get; set; }
        public bool IsPriorityOrder { get; set; }
    }
}
