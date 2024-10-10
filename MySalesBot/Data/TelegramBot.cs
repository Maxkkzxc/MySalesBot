using Telegram.Bot;
using Telegram.Bots.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace MyApp.Data
{
    public class TelegramBot
    {
        private readonly TelegramBotClient _botClient;

        public TelegramBot(IConfiguration configuration)
        {
            var token = configuration["TelegramBotToken"];
            _botClient = new TelegramBotClient(token);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var cts = new CancellationTokenSource();

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = { } }, 
                cts.Token
            );

            await Task.Delay(-1, cts.Token);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Привет! Я бот.");
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
}
