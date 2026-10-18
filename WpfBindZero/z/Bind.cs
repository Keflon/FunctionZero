using FunctionZero.ExpressionParserZero.Exceptions;
using FunctionZero.ExpressionParserZero.Operands;
using FunctionZero.ExpressionParserZero.Tokens;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Markup;

namespace FunctionZero.Wpf.zBind.z
{
    [ContentProperty(nameof(Expression))]
    public class Bind : MarkupExtension
    {
        public string? Expression { get; set; }

        public BindingMode Mode { get; set; } = BindingMode.Default;

        public object? Source { get; set; }

        public object? ConstantResult { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return CreateBinding().ProvideValue(serviceProvider);
        }

        internal BindingBase CreateBinding()
        {
            if (string.IsNullOrWhiteSpace(Expression))
                throw new XamlParseException("ZeroBind requires 'Expression' property to be set");

            var expressionParser = ExpressionParserZero.Binding.ExpressionParserFactory.GetExpressionParser();

            try
            {
                var compiledExpression = expressionParser.Parse(Expression);
                var bindingLookup = new List<string>();
                var multiBinding = new MultiBinding { Mode = Mode };

                foreach (IToken item in compiledExpression.RpnTokens)
                {
                    if (item is Operand { Type: OperandType.Variable } operand)
                    {
                        string variableName = operand.ToString();
                        if (!bindingLookup.Contains(variableName))
                        {
                            var binding = new Binding(variableName)
                            {
                                Mode = Mode == BindingMode.OneTime ? BindingMode.OneTime : BindingMode.OneWay
                            };
                            if (Source != null)
                                binding.Source = Source;

                            bindingLookup.Add(variableName);
                            multiBinding.Bindings.Add(binding);
                        }
                    }
                }

                var converter = new EvaluatorMultiConverter(bindingLookup, compiledExpression, this is MultiBind);
                multiBinding.Converter = converter;

                if (converter.WriteBackSourceIndex >= 0 &&
                    Mode is BindingMode.Default or BindingMode.TwoWay or BindingMode.OneWayToSource)
                {
                    ((Binding)multiBinding.Bindings[converter.WriteBackSourceIndex]).Mode = Mode;
                }

                if (bindingLookup.Count == 0)
                {
                    var stack = compiledExpression.Evaluate(null);
                    if (this is MultiBind)
                    {
                        var results = new List<object?>();
                        while (stack.Count > 0)
                            results.Add(stack.Pop().GetValue());
                        ConstantResult = results;
                    }
                    else
                    {
                        ConstantResult = stack.Pop().GetValue();
                    }

                    return new Binding(nameof(ConstantResult))
                    {
                        Mode = BindingMode.OneTime,
                        Source = this
                    };
                }

                return multiBinding;
            }
            catch (ExpressionParserException ex)
            {
                string problem =
                    $"z:Bind expression '{Expression}' failed at offset {ex.Offset}: {ex.Message}{Environment.NewLine}" +
                    "If the expression contains commas, enclose it in quotes so the XAML parser does not truncate it.";

                Debug.WriteLine(problem);
                ConstantResult = problem;
                return new Binding(nameof(ConstantResult))
                {
                    Mode = BindingMode.OneTime,
                    Source = this
                };
            }
        }
    }
}
