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
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddHttpClient("MyApiClient", client =>
            {
                client.BaseAddress = new Uri(config["ApiSettings:BaseUrl"]);
            });

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<DrinksPage>();
            builder.Services.AddTransient<OrdersPage>();
            builder.Services.AddTransient<AddDrinkPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
