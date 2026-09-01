namespace FunctionZero.ExpressionParserZero.Operands
{
    /// <summary>
    /// Represents an operand that exposes a writable reference. Implemented by indexed-reference operands so
    /// external code (such as binding adapters) can assign through the operand when an expression evaluates to a reference.
    /// </summary>
    public interface IWritableOperand
    {
        /// <summary>
        /// Set the underlying value represented by this operand.
        /// </summary>
        /// <param name="value">The value to assign.</param>
        void SetValue(object value);
    }
}
