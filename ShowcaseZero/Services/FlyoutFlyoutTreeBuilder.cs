using FunctionZero.Maui.MvvmZero;
using SampleApp.Mvvm.ViewModels;
using System.Collections.ObjectModel;
using System.Xml;

namespace FunctionZero.Maui.Showcase.Services
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

                    var result = ObjectGraphZero.XmlDeserializer.BuildObjectGraphFromXml(reader, _factory).Result;

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
