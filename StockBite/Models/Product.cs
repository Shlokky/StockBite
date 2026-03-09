namespace StockBite.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public ICollection<VendorProduct> VendorProducts { get; set; } = new List<VendorProduct>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
