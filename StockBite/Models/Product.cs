using StockBite.Models;

namespace StockBite.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
