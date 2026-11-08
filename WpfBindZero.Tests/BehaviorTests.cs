using FunctionZero.Wpf.zBind.z;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace FunctionZero.Wpf.BindZero.Tests
{
    [TestClass]
    public class BehaviorTests
    {
        [TestMethod]
        public void Function_ThrowsWhenExpressionIsMissing()
        {
            var function = new Function();

            Assert.ThrowsExactly<XamlParseException>(() => function.ProvideValue(null!));
        }

        [STATestMethod]
        public void TapTrigger_ButtonClick_EvaluatesAgainstDataContext()
        {
            var source = new ActionViewModel();
            var button = new Button { DataContext = source };
            var trigger = new TapTrigger
            {
                TapAction = CreateFunction("Count = Count + 1")
            };
            Interaction.GetBehaviors(button).Add(trigger);

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.AreEqual(1, source.Count);
        }

        [STATestMethod]
        public void TapTrigger_UsesCurrentDataContext()
        {
            var first = new ActionViewModel();
            var second = new ActionViewModel();
            var button = new Button { DataContext = first };
            var trigger = new TapTrigger
            {
                TapAction = CreateFunction("Count = Count + 1")
            };
            Interaction.GetBehaviors(button).Add(trigger);
            button.DataContext = second;

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.AreEqual(0, first.Count);
            Assert.AreEqual(1, second.Count);
        }

        [STATestMethod]
        public void TapTrigger_ExplicitSource_IgnoresDataContext()
        {
            var dataContext = new ActionViewModel();
            var explicitSource = new ActionViewModel();
            var button = new Button { DataContext = dataContext };
            var trigger = new TapTrigger
            {
                TapAction = CreateFunction("Count = Count + 1", explicitSource)
            };
            Interaction.GetBehaviors(button).Add(trigger);

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.AreEqual(0, dataContext.Count);
            Assert.AreEqual(1, explicitSource.Count);
        }

        [STATestMethod]
        public void TapTrigger_Detach_StopsEvaluation()
        {
            var source = new ActionViewModel();
            var button = new Button { DataContext = source };
            var behaviors = Interaction.GetBehaviors(button);
            var trigger = new TapTrigger
            {
                TapAction = CreateFunction("Count = Count + 1")
            };
            behaviors.Add(trigger);
            behaviors.Remove(trigger);

            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.AreEqual(0, source.Count);
        }

        [STATestMethod]
        public void EdgeTrigger_ConditionChanges_EvaluateRisingAndFalling()
        {
            var source = new ActionViewModel();
            var host = new Border { DataContext = source };
            var trigger = new EdgeTrigger
            {
                Rising = CreateFunction("Count = Count + 10"),
                Falling = CreateFunction("Count = Count + 1")
            };
            Interaction.GetBehaviors(host).Add(trigger);
            Assert.AreEqual(1, source.Count);

            trigger.Condition = true;
            trigger.Condition = false;

            Assert.AreEqual(12, source.Count);
        }

        [STATestMethod]
        public void EdgeTrigger_ActionAddedForCurrentCondition_EvaluatesImmediately()
        {
            var source = new ActionViewModel();
            var host = new Border { DataContext = source };
            var trigger = new EdgeTrigger { Condition = true };
            Interaction.GetBehaviors(host).Add(trigger);

            trigger.Rising = CreateFunction("Count = Count + 10");

            Assert.AreEqual(10, source.Count);
        }

        private static TreeAndSource CreateFunction(string expression, object? source = null)
        {
            return (TreeAndSource)new Function
            {
                Expression = expression,
                Source = source
            }.ProvideValue(null!);
        }

        private sealed class ActionViewModel
        {
            public int Count { get; set; }
        }
    }
}
