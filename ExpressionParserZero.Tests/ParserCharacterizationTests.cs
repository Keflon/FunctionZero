using FunctionZero.ExpressionParserZero;
using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Exceptions;
using FunctionZero.ExpressionParserZero.Operands;
using FunctionZero.ExpressionParserZero.Parser;
using System.Collections.Concurrent;

namespace ExpressionParserZero.Tests
{
    [TestClass]
    public sealed class ParserCharacterizationTests
    {
        [TestMethod]
        public void ProducesExpectedRpnForOperatorPrecedence()
        {
            var tree = new ExpressionParser().Parse("1 + 2 * 3");

            CollectionAssert.AreEqual(
                new[] { "1", "2", "3", "*", "+" },
                tree.RpnTokens.Select(token => token.ToString()).ToArray());
        }

        [TestMethod]
        public void EvaluatesUnaryOperatorsAndGrouping()
        {
            Assert.AreEqual(-9, Evaluate("-(1 + 2) * 3"));
            Assert.AreEqual(true, Evaluate("!(1 > 2)"));
        }

        [TestMethod]
        public void EvaluatesCast()
        {
            Assert.AreEqual(2.0, Evaluate("(Double)2"));
        }

        [TestMethod]
        public void EvaluatesAssignment()
        {
            var host = new ParserModel();

            var result = Evaluate("Value = 2 + 3", host);

            Assert.AreEqual(5, result);
            Assert.AreEqual(5, host.Value);
        }

        [TestMethod]
        public void EvaluatesNestedGroupingAndIndexing()
        {
            var host = new ParserModel { Values = new[] { 2, 4, 6 }, Index = 1 };

            Assert.AreEqual(10, Evaluate("(Values[(Index + 1) - 1] * 2) + 2", host));
        }

        [TestMethod]
        public void ShortCircuitSkipsRightExpression()
        {
            var host = new ParserModel { Values = new[] { 1 } };

            Assert.AreEqual(false, Evaluate("false && (Values[99] > 0)", host));
            Assert.AreEqual(true, Evaluate("true || (Values[99] > 0)", host));
        }

        [TestMethod]
        [DataRow("(1 + 2")]
        [DataRow("1 + 2)")]
        [DataRow("1 2")]
        [DataRow("1 +")]
        public void RejectsMalformedExpressions(string expression)
        {
            Assert.ThrowsExactly<ExpressionParserException>(() => new ExpressionParser().Parse(expression));
        }

        [TestMethod]
        public void ConfiguredParserSupportsConcurrentParsing()
        {
            var parser = new ExpressionParser();
            parser.RegisterFunction(
                "AddValues",
                (stack, store, position) =>
                {
                    var second = OperatorActions.PopAndResolve(stack, store);
                    var first = OperatorActions.PopAndResolve(stack, store);
                    stack.Push(new Operand(OperandType.Int, (int)first.GetValue() + (int)second.GetValue()));
                },
                2);
            var failures = new ConcurrentQueue<Exception>();

            Parallel.For(
                0,
                200,
                value =>
                {
                    try
                    {
                        var host = new ParserModel { Value = value, Values = new[] { value }, Index = 0 };
                        var store = new PocoBackingStore(host);
                        var stack = parser.Parse("AddValues(Values[Index], Value)").Evaluate(store);
                        var result = OperatorActions.PopAndResolve(stack, store).GetValue();
                        if (!Equals(value * 2, result))
                            throw new AssertFailedException($"Expected {value * 2}, but received {result}.");
                    }
                    catch (Exception exception)
                    {
                        failures.Enqueue(exception);
                    }
                });

            Assert.AreEqual(0, failures.Count, failures.TryPeek(out var failure) ? failure.ToString() : null);
        }

        private static object Evaluate(string expression, ParserModel? host = null)
        {
            var parser = new ExpressionParser();
            var store = new PocoBackingStore(host ?? new ParserModel());
            var stack = parser.Parse(expression).Evaluate(store);
            return OperatorActions.PopAndResolve(stack, store).GetValue();
        }

        private sealed class ParserModel
        {
            public int Value { get; set; }
            public int[] Values { get; set; } = Array.Empty<int>();
            public int Index { get; set; }
        }
    }
}
