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
        private readonly TelegramBotClient _botClient;
        private readonly IServiceScopeFactory _scopeFactory;
        private List<OrderItem> _cart;
        private string _location;
        private DateTime _pickupTime;
        private int _currentDrinkId;

        public TelegramBot(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            var token = configuration["TelegramBotToken"];
            _botClient = new TelegramBotClient(token);
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
            if (update.Type == UpdateType.Message && update.Message.Text != null)
            {
                if (update.Message.Text.ToLower() == "/start")
                {
                    _cart.Clear();
                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Купить", "buy")
                    });

                    await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Привет! Я бот. Нажмите 'Купить', чтобы начать.", replyMarkup: inlineKeyboard);
                    return;
                }
                else if (int.TryParse(update.Message.Text, out int quantity))
                {
                    if (await ValidateDrinkQuantity(botClient, quantity))
                    {
                        AddToCart(quantity);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Вы добавили {quantity} штук в корзину.");

                        var inlineKeyboard = new InlineKeyboardMarkup(new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Готово", "done"),
                            InlineKeyboardButton.WithCallbackData("Выбрать другой напиток", "buy")
                        });
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Что вы хотите сделать дальше?", replyMarkup: inlineKeyboard);
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Вы пытаетесь добавить больше напитков, чем есть в наличии.");
                    }
                    return;
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery.Data == "buy")
                {
                    await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Вы выбрали 'Купить'. Давайте посмотрим, какие напитки доступны.");

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var drinks = await context.Drinks.Where(d => d.Stock > 0).ToListAsync();
                        var buttons = drinks.Select(d => InlineKeyboardButton.WithCallbackData(
                            $"{d.Description} {d.Price.ToString("0.##")} BYN ({d.Stock})",
                            $"drink_{d.Id}"
                        )).ToArray();

                        var inlineKeyboard = new InlineKeyboardMarkup(buttons.Select(b => new[] { b }).ToArray());

                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Доступны напитки фирмы *:", replyMarkup: inlineKeyboard);
                    }

                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("drink_"))
                {
                    _currentDrinkId = int.Parse(update.CallbackQuery.Data.Split('_')[1]);
                    await RequestDrinkQuantity(botClient, update.CallbackQuery.Message.Chat.Id);
                    return;
                }

                if (update.CallbackQuery.Data == "done")
                {
                    if (_cart.Count == 0)
                    {
                        await botClient.SendTextMessageAsync(update.CallbackQuery.Message.Chat.Id, "Ваша корзина пуста. Пожалуйста, добавьте напитки перед оформлением заказа.");
                        return;
                    }

                    await RequestPickupLocation(botClient, update.CallbackQuery.Message.Chat.Id);
                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("location_"))
                {
                    _location = update.CallbackQuery.Data.Split('_')[1];
                    await RequestPickupTime(botClient, update.CallbackQuery.Message.Chat.Id);
                    return;
                }

                if (update.CallbackQuery.Data.StartsWith("time_"))
                {
                    _pickupTime = DateTime.Parse(update.CallbackQuery.Data.Split('_')[1]);
                    await CompleteOrder(botClient, update.CallbackQuery.Message.Chat.Id);
                    return;
                }
            }
        }

        private async Task RequestDrinkQuantity(ITelegramBotClient botClient, long chatId)
        {
            await botClient.SendTextMessageAsync(chatId, "Сколько штук? Напишите число:");
        }

        private void AddToCart(int quantity)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var drink = context.Drinks.Find(_currentDrinkId);

                if (drink != null)
                {
                    var existingItem = _cart.FirstOrDefault(item => item.DrinkId == drink.Id);
                    if (existingItem != null)
                    {
                        int newQuantity = existingItem.Quantity + quantity;

                        if (newQuantity <= drink.Stock)
                        {
                            existingItem.Quantity = newQuantity;
                        }
                        else
                        {
                            throw new InvalidOperationException("Вы пытаетесь добавить больше напитков, чем есть в наличии.");
                        }
                    }
                    else
                    {
                        var orderItem = new OrderItem
                        {
                            DrinkId = drink.Id,
                            Quantity = quantity
                        };
                        _cart.Add(orderItem);
                    }
                }
            }
        }

        private async Task<bool> ValidateDrinkQuantity(ITelegramBotClient botClient, int quantity)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var drink = await context.Drinks.FindAsync(_currentDrinkId);

                if (drink != null)
                {
                    var existingItem = _cart.FirstOrDefault(item => item.DrinkId == drink.Id);
                    int totalInCart = existingItem != null ? existingItem.Quantity : 0;

                    return totalInCart + quantity <= drink.Stock;
                }
            }
            return false;
        }

        private async Task RequestPickupLocation(ITelegramBotClient botClient, long chatId)
        {
            var locations = new[] { "место1", "место2", "место3" };
            var locationButtons = locations.Select(loc => InlineKeyboardButton.WithCallbackData(loc, $"location_{loc}")).ToArray();
            var locationKeyboard = new InlineKeyboardMarkup(locationButtons.Select(b => new[] { b }).ToArray());
            await botClient.SendTextMessageAsync(chatId, "Выбери место получения:", replyMarkup: locationKeyboard);
        }

        private async Task RequestPickupTime(ITelegramBotClient botClient, long chatId)
        {
            var timeButtons = new List<InlineKeyboardButton>();
            for (int hour = 17; hour < 21; hour++)
            {
                for (int minute = 0; minute < 60; minute += 10)
                {
                    var time = $"{hour:D2}:{minute:D2}";
                    timeButtons.Add(InlineKeyboardButton.WithCallbackData(time, $"time_{time}"));
                }
            }

            var timeKeyboard = new InlineKeyboardMarkup(timeButtons.Select(b => new[] { b }).ToArray());
            await botClient.SendTextMessageAsync(chatId, "Выбери место получения:", replyMarkup: timeKeyboard);
        }

        private async Task CompleteOrder(ITelegramBotClient botClient, long chatId)
        {
            if (_cart.Count == 0)
            {
                await botClient.SendTextMessageAsync(chatId, "Ваша корзина пуста. Пожалуйста, добавьте напитки перед оформлением заказа.");
                return;
            }

            var userName = (await botClient.GetChatAsync(chatId)).Username ?? "неизвестный";
            var userId = chatId.ToString();

            var order = new Order
            {
                Items = _cart,
                PickupLocation = _location,
                PickupTime = _pickupTime,
                UserName = userName,
                UserId = userId,
                OrderDate = DateTime.Now
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                foreach (var item in _cart)
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

            ResetBotState();

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Купить", "buy")
            });

            await botClient.SendTextMessageAsync(chatId, "Привет! Я бот. Нажмите 'Купить', чтобы начать.", replyMarkup: inlineKeyboard);
        }

        private void ResetBotState()
        {
            _cart.Clear();
            _location = null;
            _pickupTime = DateTime.MinValue;
            _currentDrinkId = 0;
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
}
