using FunctionZero.Maui.MvvmZero;
using FunctionZero.ObjectGraphZero.Factory;
using MvvmZeroFlyout.Mvvm.PageViewModels;
using MvvmZeroFlyout.Mvvm.ViewModels;
using System.Collections.ObjectModel;

namespace MvvmZeroFlyout.Services
{
    public class TreeFactory : IObjectFactory
    {
        private readonly IPageServiceZero _pageService;

        private readonly Dictionary<string, Action<object>> _actionLookup;

        public TreeFactory(IPageServiceZero pageService, ReadyPageVm readyPageVm, SteadyPageVm steadyPageVm, GoPageVm goPageVm)
        {
            _pageService = pageService;

            // Set up any MultiPage intstances that will be used by an action in the _actionLookup.
            var multiPageViewModels = new ObservableCollection<object> { readyPageVm, steadyPageVm, goPageVm };
            var multiPage = _pageService.GetMultiPage(VmInitializer, multiPageViewModels);

            _actionLookup = new Dictionary<string, Action<object>>
            {
                // A bug in TabbedPage causes extra callbacks to App.Current.DescendantAdded if the ItemsSource is replaced,
                // including if it contains th same data as the previous. Setting ~.Detail to the same MultiPage will not cause this bug
                // when the HomePage is selected / reselected. It's also better for memory-churn and performance this way.
                {"HomePage",        (flyoutItemVm) => _pageService.FlyoutController.Detail = multiPage },
                {"DetailOnePage",   (flyoutItemVm) => _pageService.FlyoutController.SetDetailVm<DetailOnePageVm>(true, vm => { }) },
                {"DetailTwoPage",   (flyoutItemVm) => _pageService.FlyoutController.SetDetailVm<DetailTwoPageVm>(true, vm => { }) },
                {"DetailThreePage", (flyoutItemVm) => _pageService.FlyoutController.SetDetailVm<DetailThreePageVm>(true, vm => { }) },
            };

            // <summary>
            // This is called for each vm the _pageService pulls from the container as it constructs the MultiPage.
            // </summary>
            // <param name="viewModel">The ViewModel instance being provided to the MultiPage.</param>
            // <returns>True if the associated page is to be wrapped in a NavigationPage.</returns>
            bool VmInitializer(object viewModel)
            {
                //return false;
                if (viewModel is ReadyPageVm readyPageVm)
                    readyPageVm.Init("This is how to call init for the ReadyPageVm from a MultiPage VmInitializer. ");
                else if (viewModel is SteadyPageVm steadyPageVm)
                    steadyPageVm.Init("This is how to call init for the SteadyPageVm from a MultiPage VmInitializer. ");
                else if (viewModel is GoPageVm goPageVm)
                    goPageVm.Init("This is how to call init for the GoPageVm from a MultiPage VmInitializer. ");

                // Return true so the root tab-item is wrapped in a NavigationPage.
                return true;
            }
        }
        public object Result { get; private set; }

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
                            title = "[!TITLE]";

                        Action<object> nodeAction = null;
                        if (factoryData.Attributes.TryGetValue("ActionKey", out var actionVmString))
                        {
                            if (_actionLookup.TryGetValue(actionVmString, out nodeAction) == false)
                                title = "[!ACTION]" + title;
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