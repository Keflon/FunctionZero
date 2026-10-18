using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using Bind = FunctionZero.Maui.zBind.z.Bind;

namespace FunctionZero.Maui.BindZero.Tests
{
    [TestClass]
    public class BindWriteBackTests
    {
        [TestMethod]
        public void DirectProperty_TwoWay_WriteBack_Works()
        {
            var vm = new ParentViewModel();
            var converter = CreateConverter("Value", vm, vm.Value);

            converter.ConvertBack(12, [typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(12, vm.Value);
        }

        [TestMethod]
        public void NestedProperty_TwoWay_WriteBack_Works()
        {
            var vm = new ParentViewModel();
            var converter = CreateConverter("Child.Value", vm, vm.Child!.Value);

            converter.ConvertBack(12, [typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(12, vm.Child.Value);
        }

        [TestMethod]
        public void NestedUnaryExpression_TwoWay_WriteBack_Works()
        {
            var vm = new ParentViewModel();
            var converter = CreateConverter("-Child.Value", vm, vm.Child!.Value);

            converter.ConvertBack(-12, [typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(12, vm.Child.Value);
        }

        [TestMethod]
        public void NestedProperty_WriteBack_DoesNothingWhenIntermediateIsNull()
        {
            var vm = new ParentViewModel { Child = null };
            var converter = CreateConverter("Child.Value", vm, null);

            converter.ConvertBack(12, [typeof(int)], null, CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void ConvertBack_ReturnsDoNothingForEachSourceBinding()
        {
            var vm = new ParentViewModel();
            var bind = new Bind { Expression = "Value + Child.Value", Source = vm, Mode = BindingMode.TwoWay };
            var binding = (MultiBinding)((IMarkupExtension<BindingBase>)bind).ProvideValue(new TestServiceProvider());

            var result = binding.Converter.ConvertBack(12, [typeof(int), typeof(int)], null, CultureInfo.InvariantCulture);

            CollectionAssert.AreEqual(new[] { Binding.DoNothing, Binding.DoNothing }, result);
        }

        [TestMethod]
        public void Convert_ReturnsDoNothingWhenSourceBindingIsUnset()
        {
            var vm = new ParentViewModel();
            var bind = new Bind { Expression = "Value + Child.Value", Source = vm };
            var binding = (MultiBinding)((IMarkupExtension<BindingBase>)bind).ProvideValue(new TestServiceProvider());

            var result = binding.Converter.Convert(
                [vm.Value, BindableProperty.UnsetValue],
                typeof(int),
                null,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        [TestMethod]
        public void Convert_ReturnsDoNothingWhenEvaluationFails()
        {
            var vm = new TestViewModel();
            var bind = new Bind { Expression = "NumbersArray[99]", Source = vm };
            var binding = (MultiBinding)((IMarkupExtension<BindingBase>)bind).ProvideValue(new TestServiceProvider());

            var result = binding.Converter.Convert(
                [vm.NumbersArray],
                typeof(int),
                null,
                CultureInfo.InvariantCulture);

            Assert.AreSame(Binding.DoNothing, result);
        }

        private static IMultiValueConverter CreateConverter(string expression, ParentViewModel source, object? currentValue)
        {
            var bind = new Bind { Expression = expression, Source = source, Mode = BindingMode.TwoWay };
            var binding = (MultiBinding)((IMarkupExtension<BindingBase>)bind).ProvideValue(new TestServiceProvider());
            var converter = binding.Converter;
            converter.Convert([currentValue], typeof(object), null, CultureInfo.InvariantCulture);
            return converter;
        }

        private sealed class ParentViewModel
        {
            public int Value { get; set; } = 1;

            public ChildViewModel? Child { get; set; } = new();
        }

        private sealed class ChildViewModel
        {
            public int Value { get; set; } = 2;
        }
    }
}
