using System.Collections.ObjectModel;
using MyApp.Models;

namespace TelegramBotApp
{
    public partial class DrinksPage : ContentPage
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Drink> Drinks { get; set; }

        public DrinksPage()
        {
            InitializeComponent();
            _apiService = new ApiService("http://localhost:5000/api/");
            Drinks = new ObservableCollection<Drink>();
            BindingContext = this;

            LoadDrinks();
        }

        private async void LoadDrinks()
        {
            var drinks = await _apiService.GetDrinksAsync();
            foreach (var drink in drinks)
            {
                Drinks.Add(drink);
            }
        }

        public async Task UpdateDrinkList()
        {
            Drinks.Clear(); 
            var drinks = await _apiService.GetDrinksAsync(); 
            foreach (var drink in drinks)
            {
                Drinks.Add(drink); 
            }
        }


        private async void OnAddDrinkButtonClicked(object sender, EventArgs e)
        {
            var addDrinkPage = new AddDrinkPage();
            addDrinkPage.DrinkAdded += (drink) =>
            {
                Drinks.Add(drink); 
            };

            await Navigation.PushAsync(addDrinkPage);
        }

        private async void OnDeleteDrinkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Drink drink)
                {
                    await _apiService.DeleteDrinkAsync(drink.Id);
                    Drinks.Remove(drink);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}