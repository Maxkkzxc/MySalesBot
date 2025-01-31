using MyApp.Models;
using TelegramBotApp.Data;

namespace TelegramBotApp
{

    public partial class EditDrinkPage : ContentPage
    {
        private readonly ApiService _apiService;
        public event Action<Drink> DrinkEdited;
        private readonly Drink _drink;

        public EditDrinkPage(string baseUrl, Drink drink)
        {
            InitializeComponent();
            _apiService = new ApiService(baseUrl);
            _drink = drink;

            DrinkNameEntry.Text = _drink.Name;
            DrinkPriceEntry.Text = _drink.Price.ToString();
            DrinkStockEntry.Text = _drink.Stock.ToString();
            DrinkDescEntry.Text = _drink.Description;
        }

        private async void OnEditDrinkButtonClicked(object sender, EventArgs e)
        {
            _drink.Name = DrinkNameEntry.Text;
            _drink.Price = decimal.Parse(DrinkPriceEntry.Text);
            _drink.Stock = int.Parse(DrinkStockEntry.Text);
            _drink.Description = DrinkDescEntry.Text;

            await _apiService.UpdateDrinkAsync(_drink);

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