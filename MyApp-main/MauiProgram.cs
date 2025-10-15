using CommunityToolkit.Maui;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;
using System.Xml;

namespace MyApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMicrocharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<MainView>();
            builder.Services.AddSingleton<MainViewModel>();

            builder.Services.AddTransient<DetailsViewModel>();
            builder.Services.AddTransient<DetailsView>();

            builder.Services.AddSingleton<DeviceOrientationService>();
            builder.Services.AddSingleton<JSONServices>();
            builder.Services.AddSingleton<MongoUserService>();

            builder.Services.AddTransient<AddWatchViewModel>();
            builder.Services.AddTransient<AddWatchView>();

            builder.Services.AddTransient<StatsView>();
            builder.Services.AddTransient<StatsViewModel>();

            builder.Services.AddTransient<LoginView>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<UsersView>();
            builder.Services.AddTransient<UsersViewModel>();

            builder.Services.AddTransient<RegisterView>();
            builder.Services.AddTransient<RegisterViewModel>();

            return builder.Build();
        }
    }
}
