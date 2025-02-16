using ShowcaseZero.Services.BasicTreeNode;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowcaseZero.Mvvm.PageViewModels.TreeView
{
    public class TreeViewBasicPageVm : BasePageVm
    {
        public BasicRootNode RootNode { get; }
        public TreeViewBasicPageVm()
        {
            RootNode = new BasicRootNode("Root!");

            RootNode.Children.CollectionChanged += Children_CollectionChanged;

            //RootNode.Level0ChildCount = 3;
            //RootNode.Level1ChildCount = 3;
            //RootNode.Level2ChildCount = 3;
            //RootNode.Level3ChildCount = 3;
        }

        private void Children_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine(RootNode.Children.Count);
            RootNode.IsExpanded = true;
        }
    }
}