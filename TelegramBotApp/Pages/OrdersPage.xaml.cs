using Microsoft.Extensions.Configuration;
using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public partial class OrdersPage : ContentPage
    {
        private readonly ApiService _apiService;

        public OrdersPage(string baseUrl)
        {
            InitializeComponent();
            _apiService = new ApiService(baseUrl);
            BindingContext = new OrdersViewModel(baseUrl);
        }

        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            var viewModel = BindingContext as OrdersViewModel;
            if (viewModel != null)
            {
                await viewModel.LoadOrders();
            }
            refreshView.IsRefreshing = false;
        }
    }
}
