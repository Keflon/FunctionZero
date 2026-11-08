using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Windows;

namespace FunctionZero.Wpf.zBind.z
{
    public sealed class EdgeTrigger : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register(
            nameof(Condition),
            typeof(bool),
            typeof(EdgeTrigger),
            new PropertyMetadata(false, OnConditionChanged));

        public static readonly DependencyProperty RisingProperty = DependencyProperty.Register(
            nameof(Rising),
            typeof(TreeAndSource),
            typeof(EdgeTrigger),
            new PropertyMetadata(null, OnRisingChanged));

        public static readonly DependencyProperty FallingProperty = DependencyProperty.Register(
            nameof(Falling),
            typeof(TreeAndSource),
            typeof(EdgeTrigger),
            new PropertyMetadata(null, OnFallingChanged));

        public bool Condition
        {
            get => (bool)GetValue(ConditionProperty);
            set => SetValue(ConditionProperty, value);
        }

        public TreeAndSource? Rising
        {
            get => (TreeAndSource?)GetValue(RisingProperty);
            set => SetValue(RisingProperty, value);
        }

        public TreeAndSource? Falling
        {
            get => (TreeAndSource?)GetValue(FallingProperty);
            set => SetValue(FallingProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            EvaluateCurrentAction();
        }

        private static void OnConditionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ((EdgeTrigger)dependencyObject).EvaluateCurrentAction();
        }

        private static void OnRisingChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var trigger = (EdgeTrigger)dependencyObject;
            if (trigger.Condition)
                trigger.EvaluateCurrentAction();
        }

        private static void OnFallingChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var trigger = (EdgeTrigger)dependencyObject;
            if (!trigger.Condition)
                trigger.EvaluateCurrentAction();
        }

        private void EvaluateCurrentAction()
        {
            if (AssociatedObject == null)
                return;

            try
            {
                (Condition ? Rising : Falling)?.Evaluate(AssociatedObject);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
