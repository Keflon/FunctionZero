using FunctionZero.ExpressionParserZero;
using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Binding;
using FunctionZero.ExpressionParserZero.Exceptions;
using FunctionZero.ExpressionParserZero.Parser;
using System.Collections;

namespace ExpressionParserZero.Tests
{
    [TestClass]
    public sealed class IndexingTests
    {
        [TestMethod]
        public void ReadsArrayUsingVariableIndex()
        {
            var host = new IndexingModel { ItemCollection = new[] { 3, 7, 11 }, NextItem = 1 };

            Assert.AreEqual(7, Evaluate(host, "ItemCollection[NextItem]"));
        }

        [TestMethod]
        public void ReadsGenericAndNonGenericLists()
        {
            var host = new IndexingModel
            {
                Items = new List<int> { 4, 8 },
                NonGenericItems = new ArrayList { 5, 10 },
                NextItem = 1
            };

            Assert.AreEqual(8, Evaluate(host, "Items[NextItem]"));
            Assert.AreEqual(10, Evaluate(host, "NonGenericItems[NextItem]"));
        }

        [TestMethod]
        public void ReadsDictionaryUsingVariableKey()
        {
            var host = new IndexingModel
            {
                Scores = new Dictionary<string, int> { ["two"] = 2 },
                Key = "two"
            };

            Assert.AreEqual(2, Evaluate(host, "Scores[Key]"));
        }

        [TestMethod]
        public void UsesIndexedValueInLargerExpression()
        {
            var host = new IndexingModel
            {
                ItemCollection = new[] { 3, 7, 11 },
                NextItem = 2,
                Parameters = new ParametersModel { BigNumber = 10 }
            };

            Assert.AreEqual(true, Evaluate(host, "ItemCollection[NextItem] > Parameters.BigNumber"));
        }

        [TestMethod]
        public void ReadsNestedCollectionsUsingChainedIndexes()
        {
            var host = new IndexingModel
            {
                NestedItems = new List<List<int>>
                {
                    new List<int> { 1, 2 },
                    new List<int> { 3, 4 }
                }
            };

            Assert.AreEqual(4, Evaluate(host, "NestedItems[1][1]"));
        }

        [TestMethod]
        public void ReadsJaggedArrayUsingChainedIndexes()
        {
            var host = new IndexingModel
            {
                JaggedItems = new[]
                {
                    new[] { 1, 2 },
                    new[] { 3, 4 }
                }
            };

            Assert.AreEqual(3, Evaluate(host, "JaggedItems[1][0]"));
        }

        [TestMethod]
        public void ReadsMultidimensionalArrayUsingCommaSeparatedIndexes()
        {
            var matrix = new int[2, 3];
            matrix[1, 2] = 9;
            var host = new IndexingModel { Matrix = matrix };

            Assert.AreEqual(9, Evaluate(host, "Matrix[1, 2]"));
        }

        [TestMethod]
        public void AssignsArrayElementAndReturnsAssignedValue()
        {
            var host = new IndexingModel { ItemCollection = new[] { 1, 2 }, NextItem = 1 };

            var result = Evaluate(host, "ItemCollection[NextItem] = 5");

            Assert.AreEqual(5, result);
            Assert.AreEqual(5, host.ItemCollection[1]);
        }

        [TestMethod]
        public void AssignsListElement()
        {
            var host = new IndexingModel { Items = new List<int> { 1, 2 }, NextItem = 0 };

            Evaluate(host, "Items[NextItem] = 6");

            Assert.AreEqual(6, host.Items[0]);
        }

        [TestMethod]
        public void AssignsExistingAndNewDictionaryKeys()
        {
            var host = new IndexingModel
            {
                Scores = new Dictionary<string, int> { ["existing"] = 1 },
                Key = "existing"
            };

            Evaluate(host, "Scores[Key] = 7");
            host.Key = "new";
            Evaluate(host, "Scores[Key] = 8");

            Assert.AreEqual(7, host.Scores["existing"]);
            Assert.AreEqual(8, host.Scores["new"]);
        }

        [TestMethod]
        public void AssignsNestedCollectionUsingChainedIndexes()
        {
            var host = new IndexingModel
            {
                NestedItems = new List<List<int>>
                {
                    new List<int> { 1, 2 },
                    new List<int> { 3, 4 }
                }
            };

            Evaluate(host, "NestedItems[1][0] = 12");

            Assert.AreEqual(12, host.NestedItems[1][0]);
        }

        [TestMethod]
        public void AssignsMultidimensionalArrayElement()
        {
            var host = new IndexingModel { Matrix = new int[2, 3] };

            Evaluate(host, "Matrix[1, 2] = 13");

            Assert.AreEqual(13, host.Matrix[1, 2]);
        }

        [TestMethod]
        public void AssignsComparisonUsingIndexedValue()
        {
            var host = new IndexingModel
            {
                ItemCollection = new[] { 3, 11 },
                NextItem = 1,
                Parameters = new ParametersModel { BigNumber = 10 }
            };

            var result = Evaluate(host, "IsBig = (ItemCollection[NextItem] > Parameters.BigNumber)");

            Assert.AreEqual(true, result);
            Assert.IsTrue(host.IsBig);
        }

        [TestMethod]
        public void ExistingAssignmentParenthesesAndPrecedenceStillWork()
        {
            var host = new IndexingModel();

            Evaluate(host, "NextItem = (1 + 2) * 3");

            Assert.AreEqual(9, host.NextItem);
        }

        [TestMethod]
        public void ExistingFunctionCommaParsingStillWorks()
        {
            var host = new IndexingModel();
            var parser = ExpressionParserFactory.GetExpressionParser();

            Assert.AreEqual(8.0, Evaluate(host, "Pow(2.0, 3.0)", parser));
        }

        [TestMethod]
        public void ShortCircuitDoesNotEvaluateInvalidIndex()
        {
            var host = new IndexingModel { ItemCollection = new[] { 1 } };

            Assert.AreEqual(false, Evaluate(host, "false && (ItemCollection[99] > 0)"));
        }

        [TestMethod]
        [DataRow("ItemCollection[]")]
        [DataRow("ItemCollection[0]]")]
        [DataRow("ItemCollection[0")]
        [DataRow("ItemCollection[,0]")]
        [DataRow("ItemCollection[0,]")]
        public void RejectsMalformedIndexSyntax(string expression)
        {
            var parser = new ExpressionParser();

            Assert.ThrowsExactly<ExpressionParserException>(() => parser.Parse(expression));
        }

        [TestMethod]
        public void RejectsIncorrectArrayRank()
        {
            var host = new IndexingModel { Matrix = new int[2, 2] };

            Assert.ThrowsExactly<ExpressionEvaluatorException>(() => Evaluate(host, "Matrix[1]"));
        }

        [TestMethod]
        public void RejectsUnsupportedAndNullTargets()
        {
            var host = new IndexingModel { ItemCollection = null! };

            Assert.ThrowsExactly<ExpressionEvaluatorException>(() => Evaluate(host, "Parameters[0]"));
            Assert.ThrowsExactly<ExpressionEvaluatorException>(() => Evaluate(host, "ItemCollection[0]"));
        }

        [TestMethod]
        public void RejectsInvalidAndOutOfRangeIndexes()
        {
            var host = new IndexingModel { ItemCollection = new[] { 1 } };

            Assert.ThrowsExactly<ExpressionEvaluatorException>(() => Evaluate(host, "ItemCollection['one']"));
            Assert.ThrowsExactly<ExpressionEvaluatorException>(() => Evaluate(host, "ItemCollection[2]"));
        }

        [TestMethod]
        public void RejectsAssignmentToReadOnlyCollection()
        {
            var host = new IndexingModel
            {
                ReadOnlyItems = ArrayList.ReadOnly(new ArrayList { 1 })
            };

            Assert.ThrowsExactly<ExpressionEvaluatorException>(() => Evaluate(host, "ReadOnlyItems[0] = 2"));
        }

        private static object Evaluate(IndexingModel host, string expression)
        {
            return Evaluate(host, expression, new ExpressionParser());
        }

        private static object Evaluate(IndexingModel host, string expression, ExpressionParser parser)
        {
            var store = new PocoBackingStore(host);
            var stack = parser.Parse(expression).Evaluate(store);
            return OperatorActions.PopAndResolve(stack, store).GetValue();
        }

        private sealed class IndexingModel
        {
            public int[] ItemCollection { get; set; } = Array.Empty<int>();
            public int NextItem { get; set; }
            public List<int> Items { get; set; } = new List<int>();
            public IList NonGenericItems { get; set; } = new ArrayList();
            public IList ReadOnlyItems { get; set; } = new ArrayList();
            public Dictionary<string, int> Scores { get; set; } = new Dictionary<string, int>();
            public string Key { get; set; } = string.Empty;
            public List<List<int>> NestedItems { get; set; } = new List<List<int>>();
            public int[][] JaggedItems { get; set; } = Array.Empty<int[]>();
            public int[,] Matrix { get; set; } = new int[0, 0];
            public ParametersModel Parameters { get; set; } = new ParametersModel();
            public bool IsBig { get; set; }
        }

        private sealed class ParametersModel
        {
            public int BigNumber { get; set; }
        }
    }
}
