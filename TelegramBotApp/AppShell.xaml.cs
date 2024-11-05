using Microsoft.Extensions.Configuration;

namespace TelegramBotApp
{
    public partial class AppShell : Shell
    {
        private readonly IConfiguration _configuration;

        public AppShell(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute(nameof(DrinksPage), typeof(DrinksPage));
            Routing.RegisterRoute(nameof(OrdersPage), typeof(OrdersPage));
        }

        private async void OnDrinksButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//DrinksPage");
        }

        private async void OnOrdersButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//OrdersPage");
        }
    }
}
