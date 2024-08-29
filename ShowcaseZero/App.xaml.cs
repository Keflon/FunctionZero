using FunctionZero.Maui.MvvmZero;
using FunctionZero.Maui.Showcase.Mvvm.Pages.Flyout;
using Microsoft.Maui.Controls;
using ShowcaseZero.Mvvm.PageViewModels.Flyout;

namespace ShowcaseZero
{
    public partial class App : Application
    {
        private IPageServiceZero _pageService;

        public App(IPageServiceZero pageService)
        {
            _pageService = pageService;
            _pageService.Init(this);

            pageService.FlyoutController.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
            InitializeComponent();
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
