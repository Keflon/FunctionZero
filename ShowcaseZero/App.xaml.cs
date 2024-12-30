using FunctionZero.Maui.MvvmZero;
using ShowcaseZero.Localization;
using ShowcaseZero.Mvvm.PageViewModels.Flyout;

namespace ShowcaseZero
{
    public partial class App : Application
    {
        private IPageServiceZero _pageService;

        public App(IPageServiceZero pageService, LocalizationService langService)
        {
            _pageService = pageService;
            InitializeComponent();

            _pageService.Init(this);
            Task.Run(() => langService.InitAsync(this.Resources, "english")).Wait();

            //var t = langService.GetText(LocalizationStrings.E_Hello);

            pageService.FlyoutController.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            //MainPage = pageService.GetFlyoutPage<FlyoutFlyoutPageVm>();

            var page = _pageService.GetFlyoutPage<FlyoutFlyoutPageVm>();

            Window w = new Window(page);

#if WINDOWS
            w.Width = 1000;
            w.Height = 800;
#endif

            return w;
        }
    }
}
