using FunctionZero.ExpressionParserZero.Tokens;
using System;

namespace FunctionZero.ExpressionParserZero.Operands
{
    internal sealed class IndexedReferenceOperand : IOperand, IWritableOperand
    {
        private readonly Func<object> _getValue;
        private readonly Action<object> _setValue;

        public IndexedReferenceOperand(long parserPosition, OperandType type, Func<object> getValue, Action<object> setValue)
        {
            ParserPosition = parserPosition;
            Type = type;
            _getValue = getValue ?? throw new ArgumentNullException(nameof(getValue));
            _setValue = setValue ?? throw new ArgumentNullException(nameof(setValue));
        }

        public long ParserPosition { get; }
        public TokenType TokenType => TokenType.Operand;
        public OperandType Type { get; }
        public bool IsNumber => Type == OperandType.Double || Type == OperandType.Long;

        public object GetValue() => _getValue();

        public void SetValue(object value) => _setValue(value);
    }
}