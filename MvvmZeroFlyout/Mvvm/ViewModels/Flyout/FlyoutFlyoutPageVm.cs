﻿using FunctionZero.Maui.MvvmZero;
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
        }
    }
}
