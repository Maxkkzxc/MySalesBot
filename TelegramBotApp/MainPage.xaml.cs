using Microsoft.Extensions.Configuration;

namespace TelegramBotApp
{
    public partial class MainPage : ContentPage
    {
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;

        public MainPage(IConfiguration configuration)
        {
            InitializeComponent();
            _configuration = configuration;
            _apiService = new ApiService(_configuration);
        }

        private async void OnManageDrinksClicked(object sender, EventArgs e)
        {
            try
            {
                var drinksPage = new DrinksPage(_configuration);
                await Navigation.PushAsync(drinksPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void OnManageOrdersClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new OrdersPage(_configuration));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void OnAddDrinkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var addDrinkPage = new AddDrinkPage(_configuration);
                await Navigation.PushAsync(addDrinkPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}