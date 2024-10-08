namespace MySalesBot.Models
{
    public class Order
    {
        public int Id { get; set; }

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public string PickupLocation { get; set; }

        public DateTime PickupTime { get; set; }
    }
}
