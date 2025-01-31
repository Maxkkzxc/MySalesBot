using CommunityToolkit.Maui.Alerts;
using Microsoft.Extensions.Configuration;
using MyApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TelegramBotApp.Data
{
    public class OrdersViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Order> Orders { get; private set; }

        public ICommand ConfirmOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }
        public ICommand CopyCommand { get; }

        public OrdersViewModel(string baseUrl)
        {
            _apiService = new ApiService(baseUrl);
            Orders = new ObservableCollection<Order>();

            ConfirmOrderCommand = new Command<Order>(async (order) => await ConfirmOrder(order));
            CancelOrderCommand = new Command<Order>(async (order) => await CancelOrder(order));
            CopyCommand = new Command<string>(async (text) => await CopyToClipboard(text));

            LoadOrders();
        }

        public async Task LoadOrders()
        {
            try
            {
                var orders = await _apiService.GetOrdersAsync();
                Orders.Clear();

                var drinks = await _apiService.GetDrinksAsync();
                var drinksDictionary = drinks.ToDictionary(d => d.Id, d => d);

                foreach (var order in orders)
                {
                    foreach (var item in order.Items)
                    {
                        if (drinksDictionary.TryGetValue(item.DrinkId, out var drink))
                        {
                            item.Drink = drink;
                        }
                    }

                    Orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки заказов: {ex.Message}");
            }
        }

        private async Task ConfirmOrder(Order order)
        {
            try
            {
                await _apiService.ConfirmOrderAsync(order.Id);
                Orders.Remove(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подтверждения заказа: {ex.Message}");
            }
        }

        private async Task CancelOrder(Order order)
        {
            try
            {
                await _apiService.CancelOrderAsync(order.Id);
                Orders.Remove(order);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отмены заказа: {ex.Message}");
            }
        }

        private async Task CopyToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                await Clipboard.SetTextAsync(text);
                await Toast.Make($"Скопировано: {text}").Show();
            }
        }
    }
}
