using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TelegramBotApp.Data;

namespace TelegramBotApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("TelegramBotApp.admappsettings.json");
            var config = new ConfigurationBuilder()
                            .AddJsonStream(stream)
                            .Build();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
                    ?? builder.Configuration.GetConnectionString("DefaultConnection");

                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(5, 5))
                );
            });


            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddHttpClient("MyApiClient", client =>
            {
                client.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
            });

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<DrinksPage>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiSettings:BaseUrl"];
                return new DrinksPage(baseUrl);
            });
            builder.Services.AddTransient<OrdersPage>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiSettings:BaseUrl"];
                return new OrdersPage(baseUrl);
            });
            builder.Services.AddTransient<AddDrinkPage>();
            builder.Services.AddTransient<StatisticsPage>((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IConfiguration>();
                string baseUrl = config["ApiSettings:BaseUrl"];
                return new StatisticsPage(baseUrl);
            });


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
