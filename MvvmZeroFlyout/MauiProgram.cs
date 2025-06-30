using FunctionZero.Maui.Controls;
using FunctionZero.Maui.MvvmZero;
using Microsoft.Extensions.Logging;
using MvvmZeroFlyout.Mvvm.Pages;
using MvvmZeroFlyout.Mvvm.Pages.Flyout;
using MvvmZeroFlyout.Mvvm.PageViewModels;
using MvvmZeroFlyout.Services;
using ShowcaseZero.Mvvm.PageViewModels.Flyout;

namespace MvvmZeroFlyout
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
                    .MapVmToView<FlyoutFlyoutPageVm, FlyoutFlyoutPage>()

                    .MapVmToView<ReadyPageVm, ReadyPage>()
                    .MapVmToView<SteadyPageVm, SteadyPage>()
                    .MapVmToView<GoPageVm, GoPage>()
                    .MapVmToView<DetailPageVm, DetailPage>()
                    .MapVmToView<DetailOnePageVm, DetailOnePage>()
                    .MapVmToView<DetailTwoPageVm, DetailTwoPage>()
                    .MapVmToView<DetailThreePageVm, DetailThreePage>()
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

                // Top level views ...
                //.AddTransient<MultiPage<Page>, TabbedPage>()
                .AddSingleton<FlyoutPage>()
                .AddSingleton<MultiPage<Page>, AdaptedTabbedPage>()  // Use TabbedPage when  https://github.com/dotnet/maui/issues/14572 is fixed.
                .AddSingleton<FlyoutFlyoutPageVm>()
                .AddSingleton<FlyoutFlyoutPage>()

                // ViewModels ...
                .AddSingleton<ReadyPageVm>()
                .AddSingleton<SteadyPageVm>()
                .AddSingleton<GoPageVm>()
                .AddSingleton<DetailOnePageVm>()
                .AddSingleton<DetailTwoPageVm>()
                .AddSingleton<DetailThreePageVm>()

                // This is transient because it's corresponding Page can appear more 
                // than once in the visual tree and we want to ensure that each instance 
                // has its own state.
                // If we want each instance of the DetailPage to have the same state,
                // we would register it as a singleton.
                .AddTransient<DetailPageVm>()

                // Views ...
                .AddSingleton<DetailOnePage>()
                .AddSingleton<DetailTwoPage>()
                .AddSingleton<DetailThreePage>()
                .AddSingleton<ReadyPage>()
                .AddSingleton<SteadyPage>()
                .AddSingleton<GoPage>()

                // This is transient because it can appear more than once in the visual tree.
                .AddTransient<DetailPage>()

                // Services ...
                .AddSingleton<FlyoutFlyoutTreeBuilder>()
                .AddSingleton<TreeFactory>()
                ;

            return builder.Build();
        }
    }
}
