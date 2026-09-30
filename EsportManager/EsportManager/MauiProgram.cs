using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using MudBlazor.Services;
using EsportManager.Features.Credentials;
using EsportManager.Features.Devices;
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
            builder.Services.AddSingleton<CredentialStore>();
            builder.Services.AddSingleton<DeviceStore>();
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddMudServices()
                .AddMudBlazorSnackbar((config) =>
                {
                    config.PreventDuplicates = true;
                    config.ClearAfterNavigation = true;
                });

#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows => windows.OnWindowCreated(window =>
                {
                    if (window.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                    {
                        presenter.Maximize();
                    }
                }));
            });
#endif

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
