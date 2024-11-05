namespace MyApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        public string PickupLocation { get; set; }
        public DateTime PickupTime { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
}
