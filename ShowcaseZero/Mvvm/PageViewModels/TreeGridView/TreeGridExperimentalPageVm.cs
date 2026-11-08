using FunctionZero.Maui.MvvmZero;
using FunctionZero.Maui.Showcase.Services;
using SampleApp.Mvvm.ViewModels;
using ShowcaseZero.Services.BasicTreeNode;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ShowcaseZero.Mvvm.PageViewModels.TreeGridView
{
    public class TreeGridExperimentalPageVm : BasePageVm
    {
        private readonly IPageServiceZero _pageService;

        //private ObservableCollection<FlyoutItemVm>? _children;
        //public ObservableCollection<FlyoutItemVm>? Children { get => _children; set => SetProperty(ref _children, value); }

        private BasicRootNode _rootNode;
        public BasicRootNode RootNode { get => _rootNode; set => SetProperty(ref _rootNode, value); }

        public ObservableCollection<string> ListChildren { get; }

        public TreeGridExperimentalPageVm(IPageServiceZero pageService)
        {
            _pageService = pageService;

            RootNode = new BasicRootNode("Root!");

            RootNode.Children.CollectionChanged += Children_CollectionChanged;

            RootNode.Level0ChildCount = 3;
            RootNode.Level1ChildCount = 3;
            RootNode.Level2ChildCount = 3;
            RootNode.Level3ChildCount = 3;

            var listChildren = new ObservableCollection<string>();

            for(int c=0;c<1000;c++)
                listChildren.Add($"Item at index {c}");

            ListChildren = listChildren;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine(RootNode.Children.Count);
            RootNode.IsExpanded = true;
        }

    }
}
