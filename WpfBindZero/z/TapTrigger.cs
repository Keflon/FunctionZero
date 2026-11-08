using Microsoft.Xaml.Behaviors;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace FunctionZero.Wpf.zBind.z
{
    public sealed class TapTrigger : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty TapActionProperty = DependencyProperty.Register(
            nameof(TapAction),
            typeof(TreeAndSource),
            typeof(TapTrigger));

        public TreeAndSource? TapAction
        {
            get => (TreeAndSource?)GetValue(TapActionProperty);
            set => SetValue(TapActionProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is ButtonBase button)
                button.Click += OnTapped;
            else
                AssociatedObject.MouseLeftButtonUp += OnTapped;
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject is ButtonBase button)
                button.Click -= OnTapped;
            else
                AssociatedObject.MouseLeftButtonUp -= OnTapped;

            base.OnDetaching();
        }

        private void OnTapped(object sender, RoutedEventArgs e)
        {
            try
            {
                TapAction?.Evaluate(AssociatedObject);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
