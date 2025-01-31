using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Models;

namespace TelegramBotApp.Data
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private Statistics _statistics;

        public Statistics Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public StatisticsViewModel(string baseUrl)
        {
            _apiService = new ApiService(baseUrl);
            LoadStatistics();
        }
        public async Task LoadStatistics()
        {
            try
            {
                IsBusy = true;
                Statistics = await _apiService.GetStatisticsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке статистики: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
