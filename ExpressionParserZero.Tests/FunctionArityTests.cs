using FunctionZero.ExpressionParserZero.Exceptions;
using FunctionZero.ExpressionParserZero.Parser;

namespace ExpressionParserZero.Tests
{
    [TestClass]
    public sealed class FunctionArityTests
    {
        [TestMethod]
        public void AcceptsExactArgumentCount()
        {
            CreateParser().Parse("Exact(1, 2)");
        }

        [TestMethod]
        [DataRow("Range(1)")]
        [DataRow("Range(1, 2)")]
        [DataRow("Range(1, 2, 3)")]
        public void AcceptsArgumentCountWithinRange(string expression)
        {
            CreateParser().Parse(expression);
        }

        [TestMethod]
        public void AcceptsZeroArgumentFunction()
        {
            CreateParser().Parse("Zero()");
        }

        [TestMethod]
        [DataRow("Exact(1)")]
        [DataRow("Exact(1, 2, 3)")]
        [DataRow("Range()")]
        [DataRow("Range(1, 2, 3, 4)")]
        public void RejectsIncorrectArgumentCount(string expression)
        {
            var exception = Assert.ThrowsExactly<ExpressionParserException>(() => CreateParser().Parse(expression));

            Assert.AreEqual(ExpressionParserException.ExceptionCause.WrongNumberOfFunctionParameters, exception.Cause);
        }

        [TestMethod]
        [DataRow("Exact(, 2)")]
        [DataRow("Exact(1,)")]
        [DataRow("Exact(1,,2)")]
        public void RejectsEmptyArgumentSlots(string expression)
        {
            Assert.ThrowsExactly<ExpressionParserException>(() => CreateParser().Parse(expression));
        }

        [TestMethod]
        public void CountsNestedFunctionArgumentsIndependently()
        {
            CreateParser().Parse("Exact(Exact(1, 2), Exact(3, 4))");
        }

        [TestMethod]
        public void CountsFunctionInsideIndexIndependently()
        {
            CreateParser().Parse("Values[Exact(0, 1)]");
        }

        [TestMethod]
        public void ReportsFunctionStartOffsetForWrongArity()
        {
            var exception = Assert.ThrowsExactly<ExpressionParserException>(() => CreateParser().Parse("1 + Exact(1)"));

            Assert.AreEqual(4, exception.Offset);
        }

        private static ExpressionParser CreateParser()
        {
            var parser = new ExpressionParser();
            parser.RegisterFunction("Exact", (stack, store, position) => { }, 2);
            parser.RegisterFunction("Range", (stack, store, position) => { }, 1, 3);
            parser.RegisterFunction("Zero", (stack, store, position) => { }, 0);
            return parser;
        }
    }
}
