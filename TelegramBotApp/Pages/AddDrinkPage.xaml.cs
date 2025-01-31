using Microsoft.Extensions.Configuration;
using MyApp.Models;
using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public partial class AddDrinkPage : ContentPage
    {
        private readonly ApiService _apiService;

        public event Action<Drink> DrinkAdded;

        public AddDrinkPage(string baseUrl)
        {
            InitializeComponent();
            _apiService = new ApiService(baseUrl);
        }

        private async void OnSaveDrinkButtonClicked(object sender, EventArgs e)
        {
            string name = DrinkNameEntry.Text;
            decimal price = decimal.Parse(DrinkPriceEntry.Text);
            int stock = int.Parse(DrinkStockEntry.Text);
            string desc = DrinkDescEntry.Text;

            var newDrink = new Drink
            {
                Name = name,
                Price = price,
                Stock = stock,
                Description = desc
            };

            await _apiService.AddDrinkAsync(newDrink);

            if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] is DrinksPage drinksPage)
            {
                if (drinksPage.BindingContext is DrinksViewModel viewModel)
                {
                    await viewModel.LoadDrinks();
                }
            }

            await Navigation.PopAsync();
        }
    }
}