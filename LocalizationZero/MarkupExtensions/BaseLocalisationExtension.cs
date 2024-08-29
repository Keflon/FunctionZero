using FunctionZero.ExpressionParserZero.Evaluator;
using FunctionZero.ExpressionParserZero.Parser;
using LocalisationZero.Localisation;
using System.ComponentModel;
using System.Linq;

namespace LocalisationZero.MarkupExtensions
{
    public abstract class BaseLocalisationExtension<TEnum> : BindableObject, IMarkupExtension<Binding>, INotifyPropertyChanged where TEnum : Enum
    {
        private readonly string _dynamicResourceName;

        public BaseLocalisationExtension(string dynamicResourceName)
        {
            _dynamicResourceName = dynamicResourceName;
        }
        public TEnum TextId { get; set; }


        #region ArgumentsProperty


        public static readonly BindableProperty ArgumentsProperty = BindableProperty.Create(nameof(Arguments), typeof(List<object>), typeof(BaseLocalisationExtension<TEnum>), new List<object>(), BindingMode.OneWay, null, ArgumentsChanged);

        //[TypeConverter(typeof(ExpressionTreeTypeConverter))]
        public List<object> Arguments
        {
            get { return (List<object>)GetValue(ArgumentsProperty); }
            set { SetValue(ArgumentsProperty, value); }
        }

        private static void ArgumentsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var self = (BaseLocalisationExtension<TEnum>)bindable;
            if (self.Target != null)
                UpdateText(self, GetLookup(self.Target));
        }

        #endregion

        private string _text;
        public Element Target { get; set; }

        public string Text
        {
            get => _text;
            set
            {
                if (value != _text)
                {
                    _text = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
                }
            }
        }

        #region INPC

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public Binding ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            var target = provideValueTarget.TargetObject as Element;
            var property = provideValueTarget.TargetProperty as BindableProperty;

            SetLangHost(target, this);
            target.SetDynamicResource(LookupProperty, _dynamicResourceName);

            Target = target;
            Target.BindingContextChanged += 
                (s, e)=>this.BindingContext = Target.BindingContext;
            //this.SetBinding(BindingContextProperty, "Target.BindingContext");

            var b = new Binding("Text", mode: BindingMode.OneWay, source: this);

            return b;
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<Binding>).ProvideValue(serviceProvider);
        }

        public static readonly BindableProperty LangHostProperty =
    BindableProperty.CreateAttached("LangHost", typeof(BaseLocalisationExtension<TEnum>), typeof(Element), null);

        public static BaseLocalisationExtension<TEnum> GetLangHost(BindableObject view)
        {
            return (BaseLocalisationExtension<TEnum>)view.GetValue(LangHostProperty);
        }

        public static void SetLangHost(BindableObject view, BaseLocalisationExtension<TEnum> value)
        {
            view.SetValue(LangHostProperty, value);
        }


        public static readonly BindableProperty LookupProperty =
            BindableProperty.CreateAttached("Lookup", typeof(LocalisationPack), typeof(Element), null, BindingMode.OneWay, null, LookupPropertyChanged);

        private static void LookupPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            BaseLocalisationExtension<TEnum> langHost = GetLangHost(bindable);
            LocalisationPack lookup = GetLookup(bindable);
            UpdateText(langHost, lookup);
        }

        private static void UpdateText(BaseLocalisationExtension<TEnum> langHost, LocalisationPack lookup)
        {
            if (lookup != null)
            {
                // langHost.Arguments are reversed because they originate from an evaluated expression.
                var arguments = new List<object>();
                foreach (var item in langHost.Arguments)
                    arguments.Insert(0, item);

                var text = lookup.GetString((int)(object)langHost.TextId, arguments.ToArray());
                langHost.Text = text;
            }
        }

        public static LocalisationPack GetLookup(BindableObject view)
        {
            var retval = (LocalisationPack)view.GetValue(LookupProperty);
            return retval;
        }

        public static void SetLookup(BindableObject view, LocalisationPack value)
        {
            view.SetValue(LookupProperty, value);
        }

    }
}
