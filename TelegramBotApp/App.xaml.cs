using Microsoft.Extensions.Configuration;

namespace TelegramBotApp
{
    public partial class App : Application
    {
        public App(IConfiguration configuration)
        {
            InitializeComponent();
            MainPage = new AppShell(configuration);
        }
    }
}
