using Microsoft.EntityFrameworkCore;
using MyApp.Models;

namespace TelegramBotApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Drink> Drinks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Statistics> Statistics { get; set; }
        public async Task<Statistics> GetStatisticsAsync()
        {
            var statistics = await Statistics.FirstOrDefaultAsync();
            if (statistics == null)
            {
                return new Statistics
                {
                    TotalAmount = 0,
                    TotalOrders = 0,
                    TotalItemsSold = 0,
                    AverageCheck = 0
                };
            }

            return statistics;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Drink)
                .WithMany()
                .HasForeignKey(oi => oi.DrinkId);

            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Drink>()
                .Property(d => d.Price)
                .HasColumnType("DECIMAL(10, 2)");

            base.OnModelCreating(modelBuilder);
        }
    }
}
