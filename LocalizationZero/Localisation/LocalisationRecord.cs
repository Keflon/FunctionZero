using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Binding;
using FunctionZero.ExpressionParserZero.Operands;
using LocalisationZero.Interpolation;

namespace LocalisationZero.Localisation
{
    public class LocalisationRecord : IBackingStore
    {
        private readonly IDictionary<string, object> _backingStore;
        public LocalisationRecord(IEnumerable<LocalisationItem> items, params string[] argumentNames)
        {
            Items = items;

            _backingStore = new Dictionary<string, object>(argumentNames.Count());

            foreach (var item in argumentNames)
                _backingStore[item] = null;
        }

        public IEnumerable<LocalisationItem> Items { get; }

        public string GetText(object[] arguments)
        {
            //if (arguments.Length != _backingStore.Count)
            //    throw new InvalidOperationException($"Argument count mismatch. {arguments.Length} arguments, expected {_backingStore.Count}");

            int c = 0;
            foreach (var key in _backingStore.Keys)
                _backingStore[key] = arguments[c++];

            foreach (var item in Items)
            {
                // TODO: Inject an EP configured with methods such as 'GetLength(length)' tailored to the current units.
                var expression = ExpressionParserFactory.GetExpressionParser().Parse(item.ConditionExpression);

                var result = expression.Evaluate(this);

                if (result.Count != 1)
                    throw new InvalidOperationException($"Wrong number of results in expression {item.ConditionExpression}, expected 1, got {result.Count}");

                IOperand operand = result.Pop();

                if (operand.Type != OperandType.Bool)
                    throw new InvalidOperationException($"Expression {item.ConditionExpression} does not resolve to a bool");

                if ((bool)operand.GetValue() == true)
                    return PerformSubstitutions(item.LocalisedText);

            }
            return "Missing translation";
        }

        private string PerformSubstitutions(string localisedText)
        {
            try
            {
                var interpolator = new StringInterpolator(localisedText, this, ExpressionParserFactory.GetExpressionParser());

                return interpolator.Result;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public (OperandType type, object value) GetValue(string qualifiedName)
        {
            // Actually, it ought to just work.
            //if (qualifiedName.Contains("."))
            //    throw new InvalidOperationException("Dotted notation is not valid in LocationRecord");

            var value = _backingStore[qualifiedName];

            OperandType opType = GetOpType(value);

            return (opType, value);
        }

        private static OperandType GetOpType(object? value)
        {
            OperandType opType;

            if (value is sbyte)
                opType = OperandType.Sbyte;
            else if (value is byte)
                opType = OperandType.Byte;
            else if (value is short)
                opType = OperandType.Short;
            else if (value is ushort)
                opType = OperandType.Ushort;
            else if (value is int)
                opType = OperandType.Int;
            else if (value is uint)
                opType = OperandType.Uint;
            else if (value is long)
                opType = OperandType.Long;
            else if (value is ulong)
                opType = OperandType.Ulong;
            else if (value is char)
                opType = OperandType.Char;
            else if (value is float)
                opType = OperandType.Float;
            else if (value is double)
                opType = OperandType.Double;
            else if (value is bool)
                opType = OperandType.Bool;
            else if (value is decimal)
                opType = OperandType.Decimal;
            else if (value is sbyte?)
                opType = OperandType.NullableSbyte;
            else if (value is byte?)
                opType = OperandType.NullableByte;
            else if (value is short?)
                opType = OperandType.NullableShort;
            else if (value is ushort?)
                opType = OperandType.NullableUshort;
            else if (value is int?)
                opType = OperandType.NullableInt;
            else if (value is uint?)
                opType = OperandType.NullableUint;
            else if (value is long?)
                opType = OperandType.NullableLong;
            else if (value is ulong?)
                opType = OperandType.NullableUlong;
            else if (value is char?)
                opType = OperandType.NullableChar;
            else if (value is float?)
                opType = OperandType.NullableFloat;
            else if (value is double?)
                opType = OperandType.NullableDouble;
            else if (value is bool?)
                opType = OperandType.NullableBool;
            else if (value is decimal?)
                opType = OperandType.NullableDecimal;
            else if (value is string)
                opType = OperandType.String;
            else if (value is object)
                opType = OperandType.Object;
            else if (value is null)
                opType = OperandType.Null;
            else
                throw new InvalidOperationException($"Bad type: {value.GetType()}");

            return opType;
        }

        public void SetValue(string qualifiedName, object value)
        {
            // Actually, it ought to just work.
            //if (qualifiedName.Contains("."))
            //    throw new InvalidOperationException("Dotted notation is not valid in LocationRecord");

            _backingStore[qualifiedName] = value;
        }
    }
}
