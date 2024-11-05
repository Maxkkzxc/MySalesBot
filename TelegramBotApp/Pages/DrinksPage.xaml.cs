using Microsoft.Extensions.Configuration;
using MyApp.Models;
using System.Collections.ObjectModel;

namespace TelegramBotApp
{
    public partial class DrinksPage : ContentPage
    {
        private readonly IConfiguration _configuration;
        private readonly ApiService _apiService;
        public ObservableCollection<Drink> Drinks { get; set; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public DrinksPage(IConfiguration configuration)
        {
            InitializeComponent();
            _apiService = new ApiService(configuration);
            Drinks = new ObservableCollection<Drink>();
            BindingContext = this;

            LoadDrinks();
        }


        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            await UpdateDrinkList();
            refreshView.IsRefreshing = false;
        }

        private async void LoadDrinks()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var drinks = await _apiService.GetDrinksAsync();
                Drinks.Clear();
                foreach (var drink in drinks)
                {
                    Drinks.Add(drink);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateDrinkList()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                Drinks.Clear();
                var drinks = await _apiService.GetDrinksAsync();
                foreach (var drink in drinks)
                {
                    Drinks.Add(drink);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnAddDrinkButtonClicked(object sender, EventArgs e)
        {
            var addDrinkPage = new AddDrinkPage(_configuration);
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