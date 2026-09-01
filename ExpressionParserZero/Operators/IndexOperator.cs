using System;

namespace FunctionZero.ExpressionParserZero.Operators
{
    internal sealed class IndexOperator : Operator
    {
        public IndexOperator(int indexCount)
            : base(
                OperatorType.Index,
                Parser.ExpressionParser.FunctionPrecedence,
                ShortCircuitMode.None,
                (stack, backingStore, parserPosition) => OperatorActions.DoIndex(stack, backingStore, parserPosition, indexCount),
                "[]")
        {
            if (indexCount < 1)
                throw new ArgumentOutOfRangeException(nameof(indexCount));

            IndexCount = indexCount;
        }

        public int IndexCount { get; }
        public int OperandCount => IndexCount + 1;
    }
}