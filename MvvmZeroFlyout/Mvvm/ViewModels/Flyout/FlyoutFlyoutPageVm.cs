using FunctionZero.Maui.MvvmZero;
using MvvmZeroFlyout.Mvvm.PageViewModels;
using MvvmZeroFlyout.Mvvm.ViewModels;
using MvvmZeroFlyout.Services;
using System.Collections.ObjectModel;

namespace ShowcaseZero.Mvvm.PageViewModels.Flyout
{
    public class FlyoutFlyoutPageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;
        private ObservableCollection<FlyoutItemVm> _children;
        public ObservableCollection<FlyoutItemVm> Children { get => _children; set => SetProperty(ref _children, value); }

        public FlyoutFlyoutPageVm(IPageServiceZero pageService, FlyoutFlyoutTreeBuilder treeBuilder)
        {
            _pageService = pageService;

            _ = treeBuilder.CreateTreeAsync().ContinueWith((thing) => Children = thing.Result);

        }


        public override void OnOwnerPageAppearing()
        {
            base.OnOwnerPageAppearing();
            //_pageService.FlyoutController.Detail = new ContentPage();
            _pageService.FlyoutController.SetDetailMultiPage((VmInitializer) => true, typeof(ReadyPageVm), typeof(SteadyPageVm), typeof(GoPageVm));
            //_pageService.FlyoutController.Detail = _pageService.GetMultiPage((VmInitializer)=>true, typeof(ReadyPageVm), typeof(SteadyPageVm), typeof(GoPageVm));
            //_pageService.FlyoutController.SetDetailVm<TreeGridExperimentalPageVm>(true, vm => { });
            //_pageService.FlyoutController.SetDetailVm<LocalizationSamplePageVm>(true, vm => { });
        }
    }
}
