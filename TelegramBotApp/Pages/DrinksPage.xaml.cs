using MyApp.Models;
using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public partial class DrinksPage : ContentPage
    {
        private readonly string _baseUrl;

        public DrinksPage(string baseUrl)
        {
            InitializeComponent();
            _baseUrl = baseUrl;
            BindingContext = new DrinksViewModel(_baseUrl);
        }

        private async void OnAddDrinkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var addDrinkPage = new AddDrinkPage(_baseUrl);

                if (BindingContext is DrinksViewModel viewModel)
                {
                    addDrinkPage.DrinkAdded += async (drink) => await viewModel.LoadDrinks();
                }

                await Navigation.PushAsync(addDrinkPage);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void OnEditDrinkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Drink selectedDrink)
                {
                    var editDrinkPage = new EditDrinkPage(_baseUrl, selectedDrink);

                    if (BindingContext is DrinksViewModel viewModel)
                    {
                        editDrinkPage.DrinkEdited += async (drink) => await viewModel.LoadDrinks();
                    }

                    await Navigation.PushAsync(editDrinkPage);
                }
                else
                {
                    await DisplayAlert("Ошибка", "Не удалось определить выбранный напиток.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            if (BindingContext is DrinksViewModel viewModel)
            {
                await viewModel.LoadDrinks();
            }
            refreshView.IsRefreshing = false;
        }
    }
}
