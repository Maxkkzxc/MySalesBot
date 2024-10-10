using System;
using Microsoft.Maui.Controls;

namespace TelegramBotApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnManageDrinksClicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new DrinksPage());

            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
        private async void OnAddDrinkButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddDrinkPage());
        }
    }
}