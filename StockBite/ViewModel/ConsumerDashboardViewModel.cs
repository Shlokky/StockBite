using StockBite.Models;

namespace StockBite.ViewModels
{
    public class ConsumerDashboardViewModel
    {
        public string ConsumerName { get; set; } = string.Empty;
        public string ConsumerCode { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new();
        public List<Product> RecommendedProducts { get; set; } = new();
        public List<ConsumerStockItemViewModel> CurrentStock { get; set; } = new();
        public List<ConsumerStockItemViewModel> PriorityProducts { get; set; } = new();
    }
}
