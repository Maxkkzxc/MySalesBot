using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApp.Data;
using MyApp.Models;

namespace MySalesBot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<Statistics>> GetStatistics()
        {
            var statistics = await _context.Statistics.FirstOrDefaultAsync();

            if (statistics == null)
            {
                return NotFound();
            }

            return Ok(statistics);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateStatistics()
        {
            var statistics = await _context.Statistics.FirstOrDefaultAsync();

            if (statistics == null)
            {
                return NotFound();
            }

            var totalAmount = await _context.Orders
                .Include(o => o.Items)
                .SumAsync(o => o.Items.Sum(oi => oi.Quantity * (oi.Drink != null ? oi.Drink.Price : 0)));

            var totalOrders = await _context.Orders.CountAsync();

            var totalItemsSold = await _context.OrderItems.SumAsync(oi => oi.Quantity);

            var averageCheck = totalOrders > 0 ? totalAmount / totalOrders : 0;

            statistics.TotalAmount = totalAmount;
            statistics.TotalOrders = totalOrders;
            statistics.TotalItemsSold = totalItemsSold;
            statistics.AverageCheck = averageCheck;

            _context.Statistics.Update(statistics);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
