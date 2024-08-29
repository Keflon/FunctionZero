using FunctionZero.Maui.MvvmZero;
using FunctionZero.ObjectGraphZero.Factory;
using SampleApp.Mvvm.ViewModels;
using ShowcaseZero.Mvvm.PageViewModels;
using ShowcaseZero.Mvvm.PageViewModels.ListView;
using ShowcaseZero.Mvvm.PageViewModels.TreeGrid;
using ShowcaseZero.Mvvm.PageViewModels.TreeView;
using System.Collections.ObjectModel;

namespace FunctionZero.Maui.Showcase.Services
{
    public class TreeFactory : IObjectFactory
    {
        private readonly IPageServiceZero _pageService;

        private readonly Dictionary<string, Action<object>> _actionLookup;

        public TreeFactory(IPageServiceZero pageService)
        {
            _pageService = pageService;

            _actionLookup = new Dictionary<string, Action<object>>
            {
                {"HomePage",   (flyoutItemVm) => _pageService.FlyoutController.SetDetailVm<HomePageVm>(true, vm => { }) },
                {"ListView",   (flyoutItemVm) => _pageService.FlyoutController.Detail = _pageService.GetMultiPage(vm=>true, typeof(ListViewAboutPageVm), typeof(ListViewBasicPageVm))},
                {"TreeView",   (flyoutItemVm) => _pageService.FlyoutController.Detail = _pageService.GetMultiPage(vm=>true, typeof(TreeViewAboutPageVm), typeof(TreeViewBasicPageVm))},
                {"MaskView",   (flyoutItemVm) => _pageService.FlyoutController.SetDetailVm<HomePageVm>(true, vm => { }) },
                //{"MemoryTest", (flyoutItemVm) => _pageService.FlyoutController.SetDetailVm<MemoryTestPageVm>(true, vm => { }) },
                {"TreeGridExperimentalPage",   (flyoutItemVm) => _pageService.FlyoutController.Detail = _pageService.GetMultiPage(vm=>true, typeof(TreeGridExperimentalPageVm))},
            };
        }
        public object Result { get; set; }

        public bool Create(FactoryData factoryData, out object createdObject)
        {
            switch (factoryData.Type)
            {
                case "Root":
                    {
                        createdObject = new ObservableCollection<FlyoutItemVm>();
                        return true;
                    }
                case "Node":
                    {
                        string title;
                        if (factoryData.Attributes.TryGetValue("Title", out title) == false)
                            title="[!TITLE]";

                        Action<object> nodeAction = null;
                        if (factoryData.Attributes.TryGetValue("ActionKey", out var actionVmString))
                        {
                            if (_actionLookup.TryGetValue(actionVmString, out nodeAction) == false)
                                title = "[!ACTION]"+title;
                        }

                        string argument = string.Empty;
                        factoryData.Attributes.TryGetValue("Argument", out argument);

                        createdObject = new FlyoutItemVm(title, nodeAction);
                        return true;
                    }

                default:
                    throw new InvalidOperationException($"Factory cannot create object type {factoryData.Type}");
            }
        }

        public void Created(object createdObject, string untrimmedContent, object parentObject)
        {
            if (createdObject is ObservableCollection<FlyoutItemVm> root)
            {
                if (Result != null)
                    throw new InvalidOperationException("There is more than one 'Root' Element");

                Result = root;
            }
            else if (createdObject is FlyoutItemVm item)
            {
                if (parentObject is ObservableCollection<FlyoutItemVm> rootCollection)
                {
                    rootCollection.Add(item);
                }
                else if (parentObject is FlyoutItemVm parentItem)
                {
                    parentItem.Children.Add(item);
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected parentObject {parentObject}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Unexpected createdObject {createdObject}");
            }
        }

        public void FoundTextFragment(object ownerObject, string untrimmedContent)
        {

        }

        public void ResetState()
        {
            Result = null;
        }
    }
}