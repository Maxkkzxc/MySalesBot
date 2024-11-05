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

        public OrdersViewModel(IConfiguration configuration)
        {
            _apiService = new ApiService(configuration);
            Orders = new ObservableCollection<Order>();

            ConfirmOrderCommand = new Command<Order>(async (order) => await ConfirmOrder(order));
            CancelOrderCommand = new Command<Order>(async (order) => await CancelOrder(order));

            LoadOrders();
        }

        public async Task LoadOrders()
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


        private async Task ConfirmOrder(Order order)
        {
            await _apiService.ConfirmOrderAsync(order.Id);
            Orders.Remove(order);
        }

        private async Task CancelOrder(Order order)
        {
            await _apiService.CancelOrderAsync(order.Id);
            Orders.Remove(order);
        }
    }
}
