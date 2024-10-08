namespace MySalesBot.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int DrinkId { get; set; }
        public int Quantity { get; set; }
        public virtual Drink Drink { get; set; }
    }
}
