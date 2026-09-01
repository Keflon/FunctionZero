using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Xml;

namespace FunctionZero.Maui.BindZero.Tests
{
    // Used as a constructor-free target in tests that only require BindableObject type identity.
    public class TestBindableTarget : BindableObject
    {
        public static readonly BindableProperty ResultProperty = BindableProperty.Create(
            nameof(Result),
            typeof(object),
            typeof(TestBindableTarget),
            defaultBindingMode: BindingMode.TwoWay);

        public object? Result
        {
            get => GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }
    }

    // Simple view model with a range of backing stores
    public class TestViewModel
    {
        public string Name { get; set; }

        public int[] NumbersArray { get; set; }

        public int[,] NumberMatrix { get; set; }

        public System.Collections.Generic.List<int> NumbersList { get; set; }

        public System.Collections.Generic.IReadOnlyList<int> ReadOnlyNumbers { get; set; }

        public System.Collections.Generic.Dictionary<string, string> Lookup { get; set; }

        public int Index { get; set; }

        public string Key { get; set; }

        public ObservableCollection<string> Obs { get; set; }

        public TestViewModel()
        {
            NumbersArray = new[] { 1, 2, 3 };
            NumberMatrix = new[,] { { 1, 2 }, { 3, 4 } };
            NumbersList = new System.Collections.Generic.List<int> { 10, 20, 30 };
            ReadOnlyNumbers = new ReadOnlyCollection<int>(NumbersList);
            Lookup = new System.Collections.Generic.Dictionary<string, string> { { "a", "A" }, { "b", "B" } };
            Index = 1;
            Key = "a";
            Obs = new ObservableCollection<string> { "x", "y", "z" };
        }
    }

    internal class TestXmlLineInfoProvider : IXmlLineInfoProvider
    {
        public IXmlLineInfo XmlLineInfo { get; } = new XmlLineInfo();
    }
}
