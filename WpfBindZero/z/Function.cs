using FunctionZero.ExpressionParserZero.BackingStore;
using System.Windows.Markup;

namespace FunctionZero.Wpf.zBind.z
{
    [ContentProperty(nameof(Expression))]
    public sealed class Function : MarkupExtension
    {
        public string? Expression { get; set; }

        public object? Source { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Expression))
                throw new XamlParseException("Function requires 'Expression' property to be set");

            var expressionParser = ExpressionParserZero.Binding.ExpressionParserFactory.GetExpressionParser();
            var compiledExpression = expressionParser.Parse(Expression);
            object? source = Source;

            return source == null
                ? new TreeAndSource(
                    compiledExpression,
                    host => host.DataContext == null ? null : new PocoBackingStore(host.DataContext))
                : new TreeAndSource(compiledExpression, _ => new PocoBackingStore(source));
        }
    }
}
