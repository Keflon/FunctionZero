using FunctionZero.Maui.MvvmZero;
using MvvmZeroFlyout.Mvvm.PageViewModels;
using ShowcaseZero.Mvvm.PageViewModels.Flyout;

namespace MvvmZeroFlyout
{
    public partial class App : Application
    {
        private readonly IPageServiceZero _pageService;

        public App(IPageServiceZero pageService)
        {
            _pageService = pageService;

            // Don't forget to call pageService.Init, or navigation will not work properly!
            pageService.Init(this);

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var rootPage = _pageService.GetFlyoutPage<FlyoutFlyoutPageVm>();
            //rootPage.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
            rootPage.IsPresented = true;
            Window w = new Window(rootPage);

#if WINDOWS
            w.Width = 1000;
            w.Height = 800;
#endif

            return w;
        }
    }
}