using Microsoft.EntityFrameworkCore;
using MyApp.Models;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MyApp.Data
{
    public class TelegramBot
    {

        private readonly Dictionary<long, UserSession> _userSessions = new();

        private class UserSession
        {
            public List<OrderItem> Cart { get; set; } = new List<OrderItem>();
            public int CurrentDrinkId { get; set; }
            public string Location { get; set; }
            public DateTime PickupTime { get; set; }
        }

        private readonly TelegramBotClient _botClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly string _logChatId;
        private List<OrderItem> _cart;
        private string _location;
        private DateTime _pickupTime;
        private int _currentDrinkId;

        public TelegramBot(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            var token = configuration["TelegramBotToken"];
            _botClient = new TelegramBotClient(token);
            _logChatId = configuration["LogChatId"];
            _cart = new List<OrderItem>();
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
            long chatId = update.Message?.Chat?.Id ?? update.CallbackQuery?.Message?.Chat?.Id ?? 0;

            if (chatId == 0) return;

            if (!_userSessions.ContainsKey(chatId))
            {
                _userSessions[chatId] = new UserSession();
            }

            var session = _userSessions[chatId];

            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                string userMessage = update.Message.Text.ToLower();

                if (userMessage == "/start")
                {
                    session.Cart.Clear();
                    var inlineKeyboard = new InlineKeyboardMarkup(new[] { InlineKeyboardButton.WithCallbackData("Купить", "buy") });
                    await botClient.SendTextMessageAsync(chatId, "Привет! Я бот. Нажмите 'Купить', чтобы начать.", replyMarkup: inlineKeyboard);
                    return;
                }

                if (session.CurrentDrinkId == 0)
                {
                    await botClient.SendTextMessageAsync(chatId, "Сначала выберите товар, прежде чем указать количество.");
                    return;
                }

                if (userMessage == "купить")
                {
                    await ShowDrinks(botClient, chatId, session);
                    return;
                }

                if (int.TryParse(userMessage, out int quantity))
                {
                    if (quantity <= 0)
                    {
                        await botClient.SendTextMessageAsync(chatId, "Количество товара должно быть положительным числом.");
                        return;
                    }

                    if (await ValidateDrinkQuantity(botClient, quantity, session))
                    {
                        await AddToCart(quantity, session, botClient, chatId);
                        await botClient.SendTextMessageAsync(chatId, $"Вы добавили {quantity} штук в корзину.");
                        var inlineKeyboard = new InlineKeyboardMarkup(new[] {
                    InlineKeyboardButton.WithCallbackData("Готово", "done"),
                    InlineKeyboardButton.WithCallbackData("Выбрать другой товар", "buy")
                });
                        await botClient.SendTextMessageAsync(chatId, "Что вы хотите сделать дальше?", replyMarkup: inlineKeyboard);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId, "Недостаточно товара в наличии.");
                    }
                    return;
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Пожалуйста, введите корректное количество товара.");
                    return;
                }
            }

            if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery.Data == "buy")
                {
                    await ShowDrinks(botClient, chatId, session);
                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("drink_"))
                {
                    session.CurrentDrinkId = int.Parse(update.CallbackQuery.Data.Split('_')[1]);
                    await RequestDrinkQuantity(botClient, chatId);
                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("date_"))
                {
                    session.PickupTime = DateTime.Parse(update.CallbackQuery.Data.Split('_')[1]);
                    await RequestPickupTime(botClient, chatId);
                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("time_"))
                {
                    var time = update.CallbackQuery.Data.Split('_')[1];
                    session.PickupTime = session.PickupTime.Date.Add(TimeSpan.Parse(time));
                    await CompleteOrder(botClient, chatId, session);
                    return;
                }

                if (update.CallbackQuery.Data == "done")
                {
                    if (session.Cart.Count == 0)
                    {
                        await botClient.SendTextMessageAsync(chatId, "Ваша корзина пуста. Пожалуйста, добавьте товары перед оформлением заказа.");
                        return;
                    }
                    await RequestPickupLocation(botClient, chatId);
                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("location_"))
                {
                    session.Location = update.CallbackQuery.Data.Split('_')[1];
                    await RequestPickupDate(botClient, chatId);
                    return;
                }
            }
        }

        private async Task ShowDrinks(ITelegramBotClient botClient, long chatId, UserSession session)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var drinks = await context.Drinks.Where(d => d.Stock > 0).ToListAsync();

                var buttons = drinks.Select(drink =>
                    InlineKeyboardButton.WithCallbackData($"{drink.Name} ({drink.Stock} шт.)", $"drink_{drink.Id}")
                ).ToArray();

                var keyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }).ToArray());
                await botClient.SendTextMessageAsync(chatId, "Выберите товар:", replyMarkup: keyboard);
            }
        }

        private async Task RequestDrinkQuantity(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, "Сколько штук? Напишите число:");
        }

        private async Task AddToCart(int quantity, UserSession session, ITelegramBotClient botClient, long chatId)
        {
            if (quantity <= 0)
            {
                await botClient.SendTextMessageAsync(chatId, "Количество товара не может быть меньше или равно нулю.");
                return;
            }

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var drink = context.Drinks.Find(session.CurrentDrinkId);

                if (drink != null)
                {
                    var existingItem = session.Cart.FirstOrDefault(item => item.DrinkId == drink.Id);
                    if (existingItem != null)
                    {
                        int newQuantity = existingItem.Quantity + quantity;
                        if (newQuantity <= drink.Stock)
                        {
                            existingItem.Quantity = newQuantity;
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(chatId, "Вы пытаетесь добавить больше товаров, чем есть в наличии.");
                        }
                    }
                    else
                    {
                        session.Cart.Add(new OrderItem { DrinkId = drink.Id, Quantity = quantity });
                    }
                }
            }
        }



        private async Task<bool> ValidateDrinkQuantity(ITelegramBotClient botClient, int quantity, UserSession session)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var drink = await context.Drinks.FindAsync(session.CurrentDrinkId);

                if (drink != null)
                {
                    var existingItem = session.Cart.FirstOrDefault(item => item.DrinkId == drink.Id);
                    int totalInCart = existingItem != null ? existingItem.Quantity : 0;

                    if (totalInCart + quantity > drink.Stock)
                    {
                        await botClient.SendTextMessageAsync(session.Cart.First().DrinkId, "Недостаточно товара в наличии.");
                        return false;
                    }
                }
            }
            return true;
        }


        private async Task RequestPickupDate(ITelegramBotClient botClient, long chatId)
        {
            var today = DateTime.Today;
            var availableDates = new List<string>();

            for (int i = 0; i < 7; i++)
            {
                var date = today.AddDays(i);
                availableDates.Add(date.ToString("yyyy-MM-dd"));
            }

            var dateButtons = availableDates.Select(date =>
                InlineKeyboardButton.WithCallbackData(date, $"date_{date}")).ToArray();
            var dateKeyboard = new InlineKeyboardMarkup(dateButtons.Select(b => new[] { b }).ToArray());

            await botClient.SendTextMessageAsync(chatId, "Выберите дату получения:", replyMarkup: dateKeyboard);
        }


        private async Task RequestPickupLocation(ITelegramBotClient botClient, long chatId)
        {
            var locations = new[] { "OldCity(Ул.Дубко 17)", "Общага(Ул.Дзержинского 35/1)", "Договорное" };
            var locationButtons = locations.Select(loc => InlineKeyboardButton.WithCallbackData(loc, $"location_{loc}")).ToArray();
            var locationKeyboard = new InlineKeyboardMarkup(locationButtons.Select(b => new[] { b }).ToArray());
            await botClient.SendTextMessageAsync(chatId, "Выбери место получения:", replyMarkup: locationKeyboard);
        }



        private async Task RequestPickupTime(ITelegramBotClient botClient, long chatId)
        {
            var timeButtons = new List<InlineKeyboardButton>();
            for (int hour = 17; hour <= 20; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    var time = $"{hour:D2}:{minute:D2}";
                    timeButtons.Add(InlineKeyboardButton.WithCallbackData(time, $"time_{time}"));
                }
            }

            var timeKeyboard = new InlineKeyboardMarkup(timeButtons.Select(b => new[] { b }).ToArray());
            await botClient.SendTextMessageAsync(chatId, "Выберите время получения:", replyMarkup: timeKeyboard);
        }

        private async Task CompleteOrder(ITelegramBotClient botClient, long chatId, UserSession session)
        {
            if (session.Cart.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, "Ваша корзина пуста. Пожалуйста, добавьте товары перед оформлением заказа.");
                return;
            }

            var userName = (await botClient.GetChatAsync(chatId)).Username ?? "неизвестный";
            var userId = chatId.ToString();

            var order = new Order
            {
                Items = session.Cart,
                PickupLocation = session.Location,
                PickupTime = session.PickupTime,
                UserName = userName,
                UserId = userId,
                OrderDate = DateTime.Now
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var item in session.Cart)
                {
                    var drink = await context.Drinks.FindAsync(item.DrinkId);
                    if (drink != null)
                    {
                        drink.Stock -= item.Quantity;
                    }
                }

                context.Orders.Add(order);
                await context.SaveChangesAsync();
            }

            await botClient.SendTextMessageAsync(chatId, "Ваш заказ завершен.");

            ResetBotState(chatId);

            var inlineKeyboard = new InlineKeyboardMarkup(new[] {
        InlineKeyboardButton.WithCallbackData("Купить", "buy")
    });

            await botClient.SendTextMessageAsync(chatId, "Привет! Я бот. Нажмите 'Купить', чтобы начать.", replyMarkup: inlineKeyboard);
        }


        private void ResetBotState(long chatId)
        {
            if (_userSessions.ContainsKey(chatId))
            {
                _userSessions[chatId] = new UserSession();
            }
        }


        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
}
