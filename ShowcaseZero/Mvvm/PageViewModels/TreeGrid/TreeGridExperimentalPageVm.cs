using FunctionZero.Maui.MvvmZero;
using FunctionZero.Maui.Showcase.Services;
using SampleApp.Mvvm.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Xml;

namespace ShowcaseZero.Mvvm.PageViewModels.TreeGrid
{
    public class TreeGridExperimentalPageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;

        private ObservableCollection<FlyoutItemVm>? _children;
        public ObservableCollection<FlyoutItemVm>? Children { get => _children; set => SetProperty(ref _children, value); }

        private FlyoutItemVm _rootNode;
        public FlyoutItemVm RootNode { get => _rootNode; set => SetProperty(ref _rootNode, value); }

        public ObservableCollection<string> ListChildren { get; }

        public TreeGridExperimentalPageVm(IPageServiceZero pageService, FlyoutFlyoutTreeBuilder treeBuilder)
        {
            _pageService = pageService;

            //_ = treeBuilder.CreateTreeAsync().ContinueWith((thing) => Children = thing.Result);

            var listChildren = new ObservableCollection<string>();

            for(int c=0;c<1000;c++)
                listChildren.Add($"Item at index {c}");

            ListChildren = listChildren;
        }

        //protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    base.OnPropertyChanged(propertyName);

        //    if (propertyName == nameof(Children))
        //    {
        //        Debug.WriteLine("XXX");
        //        RootNode = new FlyoutItemVm("Root Node", null);
        //        foreach (var item in Children)
        //            RootNode.Children.Add(item);
        //    }
        //}
    }
}
