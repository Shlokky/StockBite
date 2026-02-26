namespace StockBite.Models
{
    public class Consumer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
