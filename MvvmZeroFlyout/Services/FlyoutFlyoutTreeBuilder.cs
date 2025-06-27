using FunctionZero.Maui.MvvmZero;
using FunctionZero.ObjectGraphZero;
using MvvmZeroFlyout.Mvvm.ViewModels;
using System.Collections.ObjectModel;
using System.Xml;

namespace MvvmZeroFlyout.Services
{
    public class FlyoutFlyoutTreeBuilder
    {
        private readonly TreeFactory _factory;

        public FlyoutFlyoutTreeBuilder(TreeFactory factory)
        {
            _factory = factory;
        }


        public async Task<ObservableCollection<FlyoutItemVm>> CreateTreeAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("FlyoutTree.xml");
                using var reader = new XmlTextReader(stream);
                {
                    _factory.ResetState();

                    var result = XmlDeserializer.BuildObjectGraphFromXml(reader, _factory).Result;

                    return (ObservableCollection<FlyoutItemVm>)result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
