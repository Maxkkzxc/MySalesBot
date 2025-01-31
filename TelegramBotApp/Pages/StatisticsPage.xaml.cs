using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public partial class StatisticsPage : ContentPage
    {
        private readonly ApiService _apiService;

        public StatisticsPage(string baseUrl)
        {
            InitializeComponent();
            _apiService = new ApiService(baseUrl);
            BindingContext = new StatisticsViewModel(baseUrl);
        }

        private async void OnRefreshRequested(object sender, EventArgs e)
        {
            var viewModel = BindingContext as StatisticsViewModel;
            if (viewModel != null)
            {
                await viewModel.LoadStatistics();
                refreshView.IsRefreshing = false;
            }
        }
    }
}