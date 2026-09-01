namespace FunctionZero.ExpressionParserZero.Operators
{
    internal sealed class FunctionInvocationOperator : Operator, IFunctionOperator
    {
        public FunctionInvocationOperator(IFunctionOperator definition, int actualParameterCount)
            : base(
                OperatorType.Function,
                definition.Precedence,
                definition.ShortCircuit,
                definition.DoOperation,
                definition.AsString)
        {
            MinParameterCount = definition.MinParameterCount;
            MaxParameterCount = definition.MaxParameterCount;
            ActualParameterCount = actualParameterCount;
        }

        public int ActualParameterCount { get; }
        public int MinParameterCount { get; }
        public int MaxParameterCount { get; }
    }
}