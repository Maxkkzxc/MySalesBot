using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace TelegramBotApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Drink> Drinks { get; set; }
        public DbSet<Order> Orders { get; set; }
    }

}
