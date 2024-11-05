namespace MyApp.Data
{
    public class TelegramBotBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TelegramBotBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var bot = new TelegramBot(_scopeFactory, configuration);
                await bot.StartAsync(stoppingToken);
            }
        }
    }
}
