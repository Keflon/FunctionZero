using FunctionZero.Maui.Controls;
using FunctionZero.Maui.MvvmZero;
using FunctionZero.Maui.Showcase.Mvvm.Pages.Flyout;
using FunctionZero.Maui.Showcase.Services;
using Microsoft.Extensions.Logging;
using ShowcaseZero.Mvvm.Pages;
using ShowcaseZero.Mvvm.Pages.ListView;
using ShowcaseZero.Mvvm.Pages.Localization;
using ShowcaseZero.Mvvm.Pages.TreeGrid;
using ShowcaseZero.Mvvm.Pages.TreeView;
using ShowcaseZero.Mvvm.PageViewModels;
using ShowcaseZero.Mvvm.PageViewModels.Flyout;
using ShowcaseZero.Mvvm.PageViewModels.ListView;
using ShowcaseZero.Mvvm.PageViewModels.Localization;
using ShowcaseZero.Mvvm.PageViewModels.TreeGrid;
using ShowcaseZero.Mvvm.PageViewModels.TreeView;

namespace ShowcaseZero
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

                    .MapVmToView<HomePageVm, HomePage>()
                    .MapVmToView<ListViewAboutPageVm, ListViewAboutPage>()
                    .MapVmToView<ListViewBasicPageVm, ListViewBasicPage>()
                    .MapVmToView<TreeViewAboutPageVm, TreeViewAboutPage>()
                    .MapVmToView<TreeViewBasicPageVm, TreeViewBasicPage>()
                    .MapVmToView<TreeGridExperimentalPageVm, TreeGridExperimentalPage>()
                    .MapVmToView<LocalizationSamplePageVm, LocalizationSamplePage>()
                    

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
                //.AddSingleton<IPageServiceZero, PageServiceZero>()
                .AddSingleton<FlyoutPage>()
                .AddTransient<MultiPage<Page>, AdaptedTabbedPage>()  // Use TabbedPage when  https://github.com/dotnet/maui/issues/14572 is fixed.


                .AddSingleton<HomePageVm>()
                .AddSingleton<HomePage>()
                .AddSingleton<ListViewAboutPageVm>()
                .AddSingleton<ListViewAboutPage>()
                .AddSingleton<ListViewBasicPageVm>()
                .AddSingleton<ListViewBasicPage>()
                .AddSingleton<TreeViewAboutPageVm>()
                .AddSingleton<TreeViewAboutPage>()
                .AddSingleton<TreeViewBasicPageVm>()
                .AddSingleton<TreeViewBasicPage>()
                .AddSingleton<FlyoutFlyoutPageVm>()
                .AddSingleton<FlyoutFlyoutPage>()
                .AddSingleton<TreeGridExperimentalPageVm>()
                .AddSingleton<TreeGridExperimentalPage>()
                .AddSingleton<LocalizationSamplePageVm>()
                .AddSingleton<LocalizationSamplePage>()

                .AddSingleton<FlyoutFlyoutTreeBuilder>()
                .AddSingleton<TreeFactory>()
                ;

            return builder.Build();
        }
    }
}
