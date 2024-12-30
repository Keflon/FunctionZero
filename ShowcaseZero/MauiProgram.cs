using FunctionZero.Maui.Controls;
using FunctionZero.Maui.MvvmZero;
using FunctionZero.Maui.Showcase.Mvvm.Pages.Flyout;
using FunctionZero.Maui.Showcase.Services;
using LocalizationZero.Localization;
using Microsoft.Extensions.Logging;
using ShowcaseZero.Localization;
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
                .AddSingleton<LocalizationService>(GetConfiguredlocalisationService)

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

        #region Language translation setup
        private static LocalizationService GetConfiguredlocalisationService(IServiceProvider provider)
        {
            var localisationService = new LocalizationService();

            localisationService.RegisterLanguage("english", new LocalizationProvider(()=>FileSystem.OpenAppPackageFileAsync("Localization/en-GB.xml"),"English"));
            localisationService.RegisterLanguage("german", new LocalizationProvider(()=>FileSystem.OpenAppPackageFileAsync("Localization/de-DE.xml"),"English"));



            //localisationService.RegisterLanguage("chinese-simplified", new LocalizationProvider(() => LanguageZH.Strings, "中文"));
            //localisationService.RegisterLanguage("dutch", new LocalizationProvider(() => LanguageNL.Strings, "Nederlands"));
            //localisationService.RegisterLanguage("english", new(() => LanguageEN.Strings, "English"));
            //localisationService.RegisterLanguage("french", new LocalizationProvider(() => LanguageFR.Strings, "Française"));
            //localisationService.RegisterLanguage("german", new LocalizationProvider(() => LanguageDE.Strings, "Deutsch"));
            //localisationService.RegisterLanguage("italian", new LocalizationProvider(() => LanguageIT.Strings, "Italiana"));
            //localisationService.RegisterLanguage("japanese", new LocalizationProvider(() => LanguageJA.Strings, "日本語"));
            //localisationService.RegisterLanguage("portugese", new LocalizationProvider(() => LanguagePT.Strings, "Portugese"));
            //localisationService.RegisterLanguage("russian", new LocalizationProvider(() => LanguageRU.Strings, "русский"));
            //localisationService.RegisterLanguage("spanish", new LocalizationProvider(() => LanguageES.Strings, "Española"));
            //localisationService.RegisterLanguage("swedish", new LocalizationProvider(() => LanguageSE.Strings, "Svenska"));

            return localisationService;
        }

        #endregion
    }
}
