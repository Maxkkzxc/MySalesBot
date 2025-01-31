using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MyApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public OrdersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.Include(o => o.Items).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            foreach (var item in order.Items)
            {
                var drink = await _context.Drinks.FindAsync(item.DrinkId);
                if (drink == null || drink.Stock < item.Quantity)
                {
                    return BadRequest($"Недостаточно напитков для {drink?.Name ?? "неизвестного напитка"}.");
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Drink)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var message = new StringBuilder();
            message.AppendLine("Завершен заказ:");
            message.AppendLine($"ID заказа: {order.Id}");
            message.AppendLine($"Заказчик: {order.UserName} (ID: {order.UserId})");
            message.AppendLine($"Место получения: {order.PickupLocation}");
            message.AppendLine($"Время получения: {order.PickupTime}");
            message.AppendLine($"Дата заказа: {order.OrderDate}");
            message.AppendLine("Состав заказа:");

            decimal totalAmount = 0;
            int totalItemsSold = 0;

            foreach (var item in order.Items)
            {
                var itemAmount = item.Quantity * (item.Drink?.Price ?? 0);
                totalAmount += itemAmount;
                totalItemsSold += item.Quantity;
                message.AppendLine($"- {item.Drink?.Name ?? "Неизвестно"} x{item.Quantity} - {itemAmount:F2} BYN");
            }

            message.AppendLine($"Общая сумма: {totalAmount:F2} BYN");

            string botToken = _configuration["Telegram:BotToken"];
            string chatId = _configuration["Telegram:ChatId"];

            using var httpClient = new HttpClient();
            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = message.ToString()
            };

            var response = await httpClient.PostAsJsonAsync(url, payload);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(500, "Ошибка отправки сообщения в Telegram.");
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            foreach (var item in order.Items)
            {
                var drink = await _context.Drinks.FindAsync(item.DrinkId);
                if (drink != null)
                {
                    drink.Stock += item.Quantity;
                }
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}