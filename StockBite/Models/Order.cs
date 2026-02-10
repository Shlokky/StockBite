namespace StockBite.Models
{
    public class Order
    {
        public int Id { get; set; }   

        public string CustomerName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
