using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Maui.Controls;
using System.Globalization;
using Bind = FunctionZero.Maui.zBind.z.Bind;

namespace FunctionZero.Maui.BindZero.Tests
{
    [TestClass]
    public class BindIndexingTests
    {
        [TestMethod]
        public void IndexedArray_TwoWay_WriteBack_Works()
        {
            var vm = new TestViewModel();
            var bind = new Bind() { Expression = "NumbersArray[1]", Source = vm, Mode = BindingMode.TwoWay };
            var sp = new TestServiceProvider();

            var bindingBase = ((IMarkupExtension<BindingBase>)bind).ProvideValue(sp);
            Assert.IsInstanceOfType(bindingBase, typeof(MultiBinding));
            var mb = (MultiBinding)bindingBase;
            var conv = (IMultiValueConverter)mb.Converter;

            // Simulate Convert to populate evaluator values
            var current = conv.Convert(new object[] { vm.NumbersArray }, typeof(object), null, CultureInfo.InvariantCulture);
            Assert.AreEqual(2, current);

            // Write back new value
            conv.ConvertBack(99, new Type[] { typeof(int) }, null, CultureInfo.InvariantCulture);
            Assert.AreEqual(99, vm.NumbersArray[1]);
        }

        [TestMethod]
        public void IndexedList_TwoWay_WriteBack_Works()
        {
            var vm = new TestViewModel();
            var bind = new Bind() { Expression = "NumbersList[1]", Source = vm, Mode = BindingMode.TwoWay };
            var sp = new TestServiceProvider();

            var bindingBase = ((IMarkupExtension<BindingBase>)bind).ProvideValue(sp);
            var mb = (MultiBinding)bindingBase;
            var conv = (IMultiValueConverter)mb.Converter;

            var current = conv.Convert(new object[] { vm.NumbersList }, typeof(object), null, CultureInfo.InvariantCulture);
            Assert.AreEqual(20, current);

            conv.ConvertBack(222, new Type[] { typeof(int) }, null, CultureInfo.InvariantCulture);
            Assert.AreEqual(222, vm.NumbersList[1]);
        }

        [TestMethod]
        public void IndexedDictionary_TwoWay_WriteBack_Works()
        {
            var vm = new TestViewModel();
            var bind = new Bind() { Expression = "Lookup['a']", Source = vm, Mode = BindingMode.TwoWay };
            var sp = new TestServiceProvider();

            var bindingBase = ((IMarkupExtension<BindingBase>)bind).ProvideValue(sp);
            var mb = (MultiBinding)bindingBase;
            var conv = (IMultiValueConverter)mb.Converter;

            var current = conv.Convert(new object[] { vm.Lookup }, typeof(object), null, CultureInfo.InvariantCulture);
            Assert.AreEqual("A", current);

            conv.ConvertBack("Z", new Type[] { typeof(string) }, null, CultureInfo.InvariantCulture);
            Assert.AreEqual("Z", vm.Lookup["a"]);
        }

        [TestMethod]
        public void MultidimensionalArray_TwoWay_WriteBack_Works()
        {
            var vm = new TestViewModel();
            var converter = CreateConverter("NumberMatrix[1, 0]", vm);

            Assert.AreEqual(3, converter.Convert([vm.NumberMatrix], typeof(object), null, CultureInfo.InvariantCulture));

            converter.ConvertBack(31, [typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(31, vm.NumberMatrix[1, 0]);
        }

        [TestMethod]
        public void DynamicIndex_SelectsAndWritesCurrentElement()
        {
            var vm = new TestViewModel { Index = 2 };
            var converter = CreateConverter("NumbersList[Index]", vm);

            Assert.AreEqual(30, converter.Convert([vm.NumbersList, vm.Index], typeof(object), null, CultureInfo.InvariantCulture));

            converter.ConvertBack(42, [typeof(int), typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(42, vm.NumbersList[2]);
        }

        [TestMethod]
        public void DynamicDictionaryKey_SelectsAndWritesCurrentValue()
        {
            var vm = new TestViewModel { Key = "b" };
            var converter = CreateConverter("Lookup[Key]", vm);

            Assert.AreEqual("B", converter.Convert([vm.Lookup, vm.Key], typeof(object), null, CultureInfo.InvariantCulture));

            converter.ConvertBack("Changed", [typeof(string), typeof(string)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual("Changed", vm.Lookup["b"]);
        }

        [TestMethod]
        public void IndexedWriteBack_ConvertsCompatibleValue()
        {
            var vm = new TestViewModel();
            var converter = CreateConverter("NumbersArray[0]", vm);
            converter.Convert([vm.NumbersArray], typeof(object), null, CultureInfo.InvariantCulture);

            converter.ConvertBack("17", [typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(17, vm.NumbersArray[0]);
        }

        [TestMethod]
        public void IndexedWriteBack_DoesNotChangeReadOnlyList()
        {
            var vm = new TestViewModel();
            var converter = CreateConverter("ReadOnlyNumbers[0]", vm);
            converter.Convert([vm.ReadOnlyNumbers], typeof(object), null, CultureInfo.InvariantCulture);

            converter.ConvertBack(99, [typeof(int)], null, CultureInfo.InvariantCulture);

            Assert.AreEqual(10, vm.ReadOnlyNumbers[0]);
        }

        [TestMethod]
        public void ObservableCollectionMutation_IsReadOnNextEvaluation()
        {
            var vm = new TestViewModel();
            var converter = CreateConverter("Obs[1]", vm);

            Assert.AreEqual("y", converter.Convert([vm.Obs], typeof(object), null, CultureInfo.InvariantCulture));

            vm.Obs[1] = "changed";

            Assert.AreEqual("changed", converter.Convert([vm.Obs], typeof(object), null, CultureInfo.InvariantCulture));
        }

        private static IMultiValueConverter CreateConverter(string expression, TestViewModel source)
        {
            var bind = new Bind { Expression = expression, Source = source, Mode = BindingMode.TwoWay };
            var binding = (MultiBinding)((IMarkupExtension<BindingBase>)bind).ProvideValue(new TestServiceProvider());
            return binding.Converter;
        }
    }
}
