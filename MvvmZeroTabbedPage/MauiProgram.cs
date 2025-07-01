using FunctionZero.Maui.Controls;
using FunctionZero.Maui.MvvmZero;
using Microsoft.Extensions.Logging;
using MvvmZeroTabbedPage.Mvvm.Pages;
using MvvmZeroTabbedPage.Mvvm.PageViewModels;

namespace MvvmZeroTabbedPage
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMvvmZero(
                serviceBuilder =>
                {
                    serviceBuilder
                    .MapVmToView<ReadyPageVm, ReadyPage>()
                    .MapVmToView<SteadyPageVm, SteadyPage>()
                    .MapVmToView<GoPageVm, GoPage>()
                    .MapVmToView<DetailPageVm, DetailPage>()

                    ;
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services
                //.AddTransient<MultiPage<Page>, TabbedPage>()
                .AddTransient<MultiPage<Page>, AdaptedTabbedPage>()
                .AddSingleton<ReadyPageVm>()
                .AddSingleton<SteadyPageVm>()
                .AddSingleton<GoPageVm>()
                .AddSingleton<ReadyPage>()
                .AddSingleton<SteadyPage>()
                .AddSingleton<GoPage>()

                .AddTransient<DetailPageVm>()
                .AddTransient<DetailPage>()

                ;

            return builder.Build();
        }
    }
}
