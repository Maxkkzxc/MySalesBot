namespace MyApp.Data
{
    public class TelegramBotBackgroundService : BackgroundService
    {
        private readonly TelegramBot _telegramBot;

        public TelegramBotBackgroundService(IConfiguration configuration)
        {
            _telegramBot = new TelegramBot(configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _telegramBot.StartAsync(stoppingToken);
        }
    }
}
