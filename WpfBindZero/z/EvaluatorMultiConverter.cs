using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Evaluator;
using FunctionZero.ExpressionParserZero.Operands;
using FunctionZero.ExpressionParserZero.Tokens;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FunctionZero.Wpf.zBind.z
{
    internal sealed class EvaluatorMultiConverter : IMultiValueConverter
    {
        private readonly List<string> _keys;
        private readonly VariableEvaluator _evaluator;
        private readonly ExpressionTree _compiledExpressionTree;
        private readonly List<ExpressionTreeNode>? _reverseExpressionParentList;
        private readonly ExpressionTree? _reverseExpressionTree;
        private readonly int _writeBackSourceIndex = -1;
        private readonly bool _isMultiBind;

        internal int WriteBackSourceIndex => _writeBackSourceIndex;

        public EvaluatorMultiConverter(ICollection<string> keys, ExpressionTree compiledExpression, bool isMultiBind)
        {
            _keys = new List<string>(keys);
            _evaluator = new VariableEvaluator(_keys);
            _compiledExpressionTree = compiledExpression;
            _isMultiBind = isMultiBind;

            var reverseExpressionTree = new ExpressionTree(new List<IToken>());
            List<ExpressionTreeNode> reverseExpressionParentList = reverseExpressionTree.RootNodeList;
            var node = _compiledExpressionTree.RootNodeList[0];

            while (node.Children.Count == 1)
            {
                reverseExpressionParentList.Add(new ExpressionTreeNode(node.Token, 1));
                reverseExpressionParentList = reverseExpressionParentList[0].Children;
                node = node.Children[0];
            }

            if (node.Token is Operand { Type: OperandType.Variable } operand)
            {
                string variableName = (string)operand.GetValue();
                _writeBackSourceIndex = _keys.IndexOf(variableName);
                if (_writeBackSourceIndex >= 0)
                {
                    _reverseExpressionParentList = reverseExpressionParentList;
                    _reverseExpressionParentList.Add(new ExpressionTreeNode(null, 0));
                    _reverseExpressionTree = reverseExpressionTree;
                }
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Array.Exists(values, value => ReferenceEquals(value, DependencyProperty.UnsetValue)))
                return Binding.DoNothing;

            _evaluator.SetValues(values);

            try
            {
                var stack = _compiledExpressionTree.Evaluate(_evaluator);
                if (_isMultiBind)
                {
                    var results = new List<object?>();
                    while (stack.Count > 0)
                        results.Add(ExpressionParserZero.OperatorActions.PopAndResolve(stack, _evaluator).GetValue());
                    return results;
                }

                var result = ExpressionParserZero.OperatorActions.PopAndResolve(stack, _evaluator).GetValue();
                return ConvertResult(result, targetType, culture);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"z:Bind Convert failed: {ex.Message}");
                return Binding.DoNothing;
            }
        }

        private static object? ConvertResult(object? value, Type targetType, CultureInfo culture)
        {
            if (value == null || targetType == typeof(object) || targetType.IsInstanceOfType(value))
                return value;

            var sourceType = value.GetType();
            var targetConverter = TypeDescriptor.GetConverter(targetType);
            if (targetConverter.CanConvertFrom(sourceType))
                return targetConverter.ConvertFrom(null, culture, value);

            var sourceConverter = TypeDescriptor.GetConverter(sourceType);
            if (sourceConverter.CanConvertTo(targetType))
                return sourceConverter.ConvertTo(null, culture, value, targetType);

            if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(targetType))
                return System.Convert.ChangeType(value, targetType, culture);

            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            var sourceResults = new object[targetTypes.Length];
            Array.Fill(sourceResults, Binding.DoNothing);

            try
            {
                if (_reverseExpressionTree != null && _reverseExpressionParentList != null && _writeBackSourceIndex >= 0)
                {
                    var operandType = GetOperandType(targetTypes[_writeBackSourceIndex], value);
                    _reverseExpressionParentList[0] = new ExpressionTreeNode(new Operand(operandType, value), 0);
                    var result = _reverseExpressionTree.Evaluate(_evaluator);
                    sourceResults[_writeBackSourceIndex] = result.Pop().GetValue();
                    return sourceResults;
                }

                var resultStack = _compiledExpressionTree.Evaluate(_evaluator);
                if (resultStack.Count > 0 && resultStack.Pop() is IWritableOperand writable)
                    writable.SetValue(value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"z:Bind ConvertBack failed: {ex.Message}");
            }

            return sourceResults;
        }

        private static OperandType GetOperandType(Type targetType, object value)
        {
            if (value == null)
                return OperandType.Null;
            if (BackingStoreHelpers.OperandTypeLookup.TryGetValue(targetType, out var targetOperandType))
                return targetOperandType;
            if (BackingStoreHelpers.OperandTypeLookup.TryGetValue(value.GetType(), out var valueOperandType))
                return valueOperandType;
            return OperandType.Object;
        }
    }
}
