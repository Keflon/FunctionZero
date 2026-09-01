using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Maui.Controls;
using System;
using System.Runtime.CompilerServices;
using Bind = FunctionZero.Maui.zBind.z.Bind;

namespace FunctionZero.Maui.BindZero.Tests
{
    [TestClass]
    public class BindProvideValueTests
    {
        [TestMethod]
        public void ProvideValue_Throws_WhenExpressionMissing()
        {
            var bind = new Bind();

            var sp = new TestServiceProvider();

            Assert.ThrowsExactly<Microsoft.Maui.Controls.Xaml.XamlParseException>(() => ((IMarkupExtension<BindingBase>)bind).ProvideValue(sp));
        }

        [TestMethod]
        public void ProvideValue_SetsMultiBindingMode_And_ReturnsMultiBindingForVariables()
        {
            var bind = new Bind() { Expression = "a + b", Mode = BindingMode.TwoWay };

            var sp = new TestServiceProvider();

            var result = ((IMarkupExtension<BindingBase>)bind).ProvideValue(sp);

            Assert.IsInstanceOfType(result, typeof(MultiBinding));
            var mb = (MultiBinding)result;
            Assert.AreEqual(BindingMode.TwoWay, mb.Mode);
            // Expect two variable bindings
            Assert.IsTrue(mb.Bindings.Count >= 2);
        }

        [TestMethod]
        public void ProvideValue_Captures_BindableTarget_From_ServiceProvider()
        {
            var bind = new Bind() { Expression = "x" };

            var bindableTarget = (TestBindableTarget)RuntimeHelpers.GetUninitializedObject(typeof(TestBindableTarget));
            var sp = new TestServiceProvider(bindableTarget);

            var result = ((IMarkupExtension<BindingBase>)bind).ProvideValue(sp);

            // BindableTarget is internal; use reflection to verify it was captured
            var prop = typeof(Bind).GetProperty("BindableTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            var target = prop.GetValue(bind) as BindableObject;
            Assert.IsNotNull(target);
        }
    }

    // Minimal service provider stubs
    internal class TestServiceProvider : IServiceProvider
    {
        private readonly IProvideValueTarget _provideValueTarget;

        public TestServiceProvider(object? targetObject = null, object? targetProperty = null)
        {
            _provideValueTarget = new TestProvideValueTarget(targetObject ?? new object(), targetProperty);
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IXmlLineInfoProvider))
                return null;
            if (serviceType == typeof(IProvideValueTarget))
                return _provideValueTarget;
            return null;
        }
    }

    internal class TestProvideValueTarget : IProvideValueTarget
    {
        public TestProvideValueTarget(object targetObject, object? targetProperty)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
        }

        public object TargetObject { get; }

        public object? TargetProperty { get; }
    }
}
