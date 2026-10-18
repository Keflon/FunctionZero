using FunctionZero.ExpressionParserZero.BackingStore;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionZero.Maui.zBind.z
{
    public class TapTrigger : Behavior<Element>
    {
        private BindableObject? _host;
        private IBackingStore? _risingBackingStore;
        private TapGestureRecognizer? _tapGestureRecognizer;
        public TapTrigger()
        {
            // TODO: 
        }
        protected override void OnAttachedTo(BindableObject host)
        {
            // TODO: Hook the appropriate event on host, depending on type, ...
            // TODO: Button -> Click event
            // TODO: TapGestureRecognizer -> Tapped event
            // TODO: And call DoAction(sender, e)
            _host = host;

            if (host is Button b)
            {
                host.BindingContextChanged += _host_BindingContextChanged;
                b.Clicked += DoAction;
            }
            else if (host is View v)
            {
                host.BindingContextChanged += _host_BindingContextChanged;
                _tapGestureRecognizer = new TapGestureRecognizer();
                _tapGestureRecognizer.Tapped += DoAction;
                v.GestureRecognizers.Add(_tapGestureRecognizer);
            }
            else
                throw new NotImplementedException("TapTrigger - host is not a View");

            base.OnAttachedTo(host);
        }

        protected override void OnDetachingFrom(Element host)
        {
            // TODO: Hook the appropriate event on host, depending on type, ...
            // TODO: Button -> Click event
            // TODO: TapGestureRecognizer -> Tapped event

            host.BindingContextChanged -= _host_BindingContextChanged;

            if (host is Button button)
                button.Clicked -= DoAction;
            else if (host is View view && _tapGestureRecognizer != null)
            {
                _tapGestureRecognizer.Tapped -= DoAction;
                view.GestureRecognizers.Remove(_tapGestureRecognizer);
            }

            _tapGestureRecognizer = null;
            _risingBackingStore = null;
            _host = null;

            base.OnDetachingFrom(host);
        }

        private void _host_BindingContextChanged(object? sender, EventArgs e)
        {
            // this.BindingContext is needed for the BindingContext of the TapAction.
            // TODO: Use a binding object.
            BindingContext = _host?.BindingContext;

            // Get the backing store from the Rising and Falling instances ...
            _risingBackingStore = _host == null ? null : TapAction?.SourceGetter(_host);
        }

        private void DoAction(object? sender, EventArgs e)
        {
            try
            {
                TapAction?.Tree?.Evaluate(_risingBackingStore);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
        public static readonly BindableProperty TapActionProperty = BindableProperty.Create(nameof(TapAction), typeof(TreeAndSource), typeof(TapTrigger), null, BindingMode.OneWay, null, TapTriggerChanged);

        public TreeAndSource? TapAction
        {
            get { return (TreeAndSource?)GetValue(TapActionProperty); }
            set { SetValue(TapActionProperty, value); }
        }

        private static void TapTriggerChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            try
            {
                var self = (TapTrigger)bindable;

                self._risingBackingStore = self._host == null ? null : self.TapAction?.SourceGetter(self._host);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
