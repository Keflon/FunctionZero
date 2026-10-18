using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Bind = FunctionZero.Wpf.zBind.z.Bind;
using MultiBindExtension = FunctionZero.Wpf.zBind.z.MultiBind;

namespace FunctionZero.Wpf.BindZero.Tests
{
    [TestClass]
    public class BindTests
    {
        [TestMethod]
        public void CreateBinding_ThrowsWhenExpressionIsMissing()
        {
            var bind = new Bind();

            Assert.ThrowsExactly<XamlParseException>(() => bind.CreateBinding());
        }

        [TestMethod]
        public void CreateBinding_CreatesOrderedBindingsForVariables()
        {
            var source = new TestViewModel();
            var bind = new Bind
            {
                Expression = "Numbers[Index] + Value + Value",
                Mode = BindingMode.TwoWay,
                Source = source
            };

            var result = bind.CreateBinding();

            Assert.IsInstanceOfType<MultiBinding>(result);
            var multiBinding = (MultiBinding)result;
            Assert.AreEqual(BindingMode.TwoWay, multiBinding.Mode);
            CollectionAssert.AreEqual(
                new[] { "Numbers", "Index", "Value" },
                multiBinding.Bindings.Cast<Binding>().Select(binding => binding.Path.Path).ToArray());
            Assert.IsTrue(multiBinding.Bindings.Cast<Binding>().All(binding => ReferenceEquals(source, binding.Source)));
        }

        [TestMethod]
        public void CreateBinding_DoesNotSetSourceWhenUsingDataContext()
        {
            var bind = new Bind { Expression = "Value" };

            var result = (MultiBinding)bind.CreateBinding();

            Assert.IsNull(((Binding)result.Bindings[0]).Source);
        }

        [TestMethod]
        public void CreateBinding_EvaluatesConstantExpression()
        {
            var bind = new Bind { Expression = "2 + 3" };

            var result = bind.CreateBinding();

            Assert.IsInstanceOfType<Binding>(result);
            Assert.AreEqual(5, bind.ConstantResult);
            Assert.AreEqual(BindingMode.OneTime, ((Binding)result).Mode);
            Assert.AreSame(bind, ((Binding)result).Source);
        }

        [TestMethod]
        public void Convert_EvaluatesExpression()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("Value * 2", source);

            var result = converter.Convert([source.Value], typeof(long), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(8, result);
        }

        [TestMethod]
        public void Convert_ReturnsDoNothingForUnsetValue()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("Value * 2", source);

            var result = converter.Convert([DependencyProperty.UnsetValue], typeof(long), null!, CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void Convert_ReturnsDoNothingWhenEvaluationFails()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("Numbers[99]", source);

            var result = converter.Convert([source.Numbers], typeof(int), null!, CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void ConvertBack_ReturnsDirectValueToChildBinding()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("Value", source);
            converter.Convert([source.Value], typeof(int), null!, CultureInfo.InvariantCulture);

            var result = converter.ConvertBack(12, [typeof(int)], null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(12, result[0]);
        }

        [TestMethod]
        public void ConvertBack_ReversesUnaryExpressionForChildBinding()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("-Child.Value", source);
            converter.Convert([source.Child.Value], typeof(int), null!, CultureInfo.InvariantCulture);

            var result = converter.ConvertBack(-12, [typeof(int)], null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(12, result[0]);
        }

        [TestMethod]
        public void ConvertBack_WritesIndexedArrayValue()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("Numbers[Index]", source);
            converter.Convert([source.Numbers, source.Index], typeof(int), null!, CultureInfo.InvariantCulture);

            var result = converter.ConvertBack(42, [typeof(int[]), typeof(int)], null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(42, source.Numbers[1]);
            CollectionAssert.AreEqual(new[] { Binding.DoNothing, Binding.DoNothing }, result);
        }

        [TestMethod]
        public void ConvertBack_WritesIndexedDictionaryValue()
        {
            var source = new TestViewModel();
            var converter = CreateConverter("Lookup[Key]", source);
            converter.Convert([source.Lookup, source.Key], typeof(string), null!, CultureInfo.InvariantCulture);

            converter.ConvertBack("Changed", [typeof(Dictionary<string, string>), typeof(string)], null!, CultureInfo.InvariantCulture);

            Assert.AreEqual("Changed", source.Lookup["a"]);
        }

        [TestMethod]
        public void MultiBind_ReturnsEveryStackResult()
        {
            var source = new TestViewModel();
            var bind = new MultiBindExtension { Expression = "Value, Index", Source = source };
            var binding = (MultiBinding)bind.CreateBinding();

            var result = binding.Converter.Convert([source.Value, source.Index], typeof(object), null!, CultureInfo.InvariantCulture);

            Assert.IsInstanceOfType<List<object?>>(result);
            CollectionAssert.AreEqual(new object?[] { source.Index, source.Value }, (List<object?>)result);
        }

        [STATestMethod]
        public void BindingEngine_ReevaluatesIndexedExpressionWhenIndexChanges()
        {
            var source = new ObservableViewModel();
            var target = new BindingTarget { DataContext = source };
            var bind = new Bind { Expression = "Numbers[Index]" };

            BindingOperations.SetBinding(target, BindingTarget.ResultProperty, bind.CreateBinding());
            Assert.AreEqual(2, target.Result);

            source.Index = 2;

            Assert.AreEqual(3, target.Result);
        }

        [STATestMethod]
        public void BindingEngine_UpdatesNestedSourceThroughChildBinding()
        {
            var source = new ObservableViewModel();
            var target = new BindingTarget { DataContext = source };
            var bind = new Bind { Expression = "-Child.Value", Mode = BindingMode.TwoWay };

            BindingOperations.SetBinding(target, BindingTarget.ResultProperty, bind.CreateBinding());
            Assert.AreEqual(-2, target.Result);

            target.Result = -12;

            Assert.AreEqual(12, source.Child.Value);
        }

        private static IMultiValueConverter CreateConverter(string expression, TestViewModel source)
        {
            var bind = new Bind { Expression = expression, Source = source, Mode = BindingMode.TwoWay };
            return ((MultiBinding)bind.CreateBinding()).Converter;
        }

        private sealed class TestViewModel
        {
            public int Value { get; set; } = 4;

            public int[] Numbers { get; } = [1, 2, 3];

            public int Index { get; set; } = 1;

            public string Key { get; set; } = "a";

            public Dictionary<string, string> Lookup { get; } = new() { ["a"] = "A", ["b"] = "B" };

            public ChildViewModel Child { get; } = new();

            public ObservableCollection<int> ObservableNumbers { get; } = [1, 2, 3];
        }

        private sealed class ChildViewModel
        {
            public int Value { get; set; } = 2;
        }

        private sealed class BindingTarget : FrameworkElement
        {
            public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(
                nameof(Result),
                typeof(object),
                typeof(BindingTarget),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            public object? Result
            {
                get => GetValue(ResultProperty);
                set => SetValue(ResultProperty, value);
            }
        }

        private sealed class ObservableViewModel : INotifyPropertyChanged
        {
            private int _index = 1;

            public event PropertyChangedEventHandler? PropertyChanged;

            public int[] Numbers { get; } = [1, 2, 3];

            public int Index
            {
                get => _index;
                set
                {
                    _index = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Index)));
                }
            }

            public ChildViewModel Child { get; } = new();
        }
    }
}
