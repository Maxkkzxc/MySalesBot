using MyApp.Models;
using System;

namespace TelegramBotApp
{
    public partial class AddDrinkPage : ContentPage
    {
        private readonly ApiService _apiService;

        public event Action<Drink> DrinkAdded;

        public AddDrinkPage()
        {
            InitializeComponent();
            _apiService = new ApiService("http://localhost:5000/api/");
        }

        private async void OnSaveDrinkButtonClicked(object sender, EventArgs e)
        {
            string name = DrinkNameEntry.Text;
            decimal price = decimal.Parse(DrinkPriceEntry.Text);
            int stock = int.Parse(DrinkStockEntry.Text);

            var newDrink = new Drink
            {
                Name = name,
                Price = price,
                Stock = stock
            };

            await _apiService.AddDrinkAsync(newDrink);

            if (Navigation.NavigationStack[Navigation.NavigationStack.Count - 2] is DrinksPage drinksPage)
            {
                await drinksPage.UpdateDrinkList(); // Метод для получения актуального списка
            }

            await Navigation.PopAsync();
        }
    }
}