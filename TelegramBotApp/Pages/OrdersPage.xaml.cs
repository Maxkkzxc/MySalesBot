using Microsoft.Extensions.Configuration;
using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public partial class OrdersPage : ContentPage
    {
        public OrdersPage(IConfiguration configuration)
        {
            InitializeComponent();
            BindingContext = new OrdersViewModel(configuration);
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