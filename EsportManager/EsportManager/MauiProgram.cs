using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using EsportManager.Features.Process;

namespace EsportManager
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddSingleton<LaunchSettings>();
            builder.Services.AddSingleton<ProcessLoggerFactory>();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices()
                .AddMudBlazorSnackbar((config) =>
                {
                    config.PreventDuplicates = true;
                    config.ClearAfterNavigation = true;
                });

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
