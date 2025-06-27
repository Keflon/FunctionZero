using FunctionZero.Maui.MvvmZero;
using MvvmZeroFlyout.Mvvm.PageViewModels;

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
            MultiPage<Page> rootPage = _pageService.GetMultiPage(VmInitializer, typeof(ReadyPageVm), typeof(SteadyPageVm), typeof(GoPageVm));
            return new Window(rootPage);
        }

        /// <summary>
        /// This is called for each vm the _pageService pulls from the container as it constructs the MultiPage.
        /// </summary>
        /// <param name="viewModel">The ViewModel instance being provided to the MultiPage.</param>
        /// <returns>True if the associated page is to be wrapped in a NavigationPage.</returns>
        private bool VmInitializer(object viewModel)
        {
            if (viewModel is ReadyPageVm readyPageVm)
            {
                readyPageVm.Init("This is how to call init for the ReadyPageVm from a MultiPage VmInitializer. ");
                return false; // Do not wrap the ReadyPage in a NavigationPage.
            }
            else if (viewModel is SteadyPageVm steadyPageVm)
            {
                steadyPageVm.Init("This is how to call init for the SteadyPageVm from a MultiPage VmInitializer. ");
                return true; // Wrap the SteadyPage in a NavigationPage.
            }
            else if (viewModel is GoPageVm goPageVm)
            {
                goPageVm.Init("This is how to call init for the GoPageVm from a MultiPage VmInitializer. ");
                return true; // Wrap the GoPage in a NavigationPage.
            }
            else // We don't know what it is, so just wrap it in a NavigationPage.
                return true;
        }
    }
}