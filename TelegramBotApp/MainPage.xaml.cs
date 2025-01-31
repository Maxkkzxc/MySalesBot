using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public partial class MainPage : ContentPage
    {
        private readonly ApiService _apiService;
        private string _baseUrl;

        public MainPage()
        {
            InitializeComponent();
            _baseUrl = LoadBaseUrl();
            _apiService = new ApiService(_baseUrl);
        }

        private string LoadBaseUrl()
        {
            try
            {
                string configPath = Path.Combine(FileSystem.AppDataDirectory, "config.json");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    var configData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                    if (configData != null && configData.TryGetValue("ApiBaseUrl", out string url))
                    {
                        return url;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки конфигурации: {ex.Message}");
            }

            return "https://default-url.com/api/";
        }

        private async void OnManageDrinksClicked(object sender, EventArgs e)
        {
            try
            {
                var drinksPage = new DrinksPage(_baseUrl);
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
                await Navigation.PushAsync(new OrdersPage(_baseUrl));
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
                var addDrinkPage = new AddDrinkPage(_baseUrl);
                await Navigation.PushAsync(addDrinkPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}
