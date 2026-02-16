namespace StockBite.Models
{
    public class VendorComparisonVM
    {
        public string ProductName { get; set; }

        public List<VendorProduct> VendorOptions { get; set; }
    }
}
