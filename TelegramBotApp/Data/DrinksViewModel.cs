using Microsoft.Maui.Controls;
using MyApp.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TelegramBotApp.Data
{
    public class DrinksViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<Drink> Drinks { get; set; }

        public ICommand DeleteDrinkCommand { get; }

        public DrinksViewModel(string baseUrl)
        {
            _apiService = new ApiService(baseUrl);
            Drinks = new ObservableCollection<Drink>();
            DeleteDrinkCommand = new Command<Drink>(async (drink) => await DeleteDrink(drink));
            LoadDrinks();
        }

        public async Task LoadDrinks()
        {
            var drinks = await _apiService.GetDrinksAsync();
            Drinks.Clear();

            foreach (var drink in drinks)
            {
                Drinks.Add(drink);
            }
        }

        private async Task DeleteDrink(Drink drink)
        {
            if (drink == null) return;

            try
            {
                await _apiService.DeleteDrinkAsync(drink.Id);
                Drinks.Remove(drink);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении напитка: {ex.Message}");
            }
        }
    }
}
