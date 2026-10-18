using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Operands;

namespace FunctionZero.Wpf.zBind.z
{
    internal sealed class VariableEvaluator : IBackingStore
    {
        private readonly IList<string> _keys;
        private object[]? _values;

        public VariableEvaluator(IList<string> keys)
        {
            _keys = keys;
        }

        public void SetValues(object[] values)
        {
            _values = values;
        }

        public (OperandType type, object value) GetValue(string qualifiedName)
        {
            int index = _keys.IndexOf(qualifiedName);
            if (index < 0)
                throw new KeyNotFoundException($"The expression variable '{qualifiedName}' has no corresponding binding.");
            if (_values == null)
                throw new InvalidOperationException("Binding values have not been supplied to the expression evaluator.");

            object value = _values[index];
            if (value == null)
                return (OperandType.Null, null!);

            return BackingStoreHelpers.OperandTypeLookup.TryGetValue(value.GetType(), out var operandType)
                ? (operandType, value)
                : (OperandType.Object, value);
        }

        public void SetValue(string qualifiedName, object value)
        {
            throw new NotSupportedException("WPF source updates are performed by the child Binding instances.");
        }
    }
}
