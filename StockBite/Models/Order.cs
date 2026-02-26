namespace StockBite.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ConsumerId { get; set; }
        public Consumer Consumer { get; set; } = null!;
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime? ApprovedAt { get; set; }
    }
}
