using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Evaluator;
using FunctionZero.ExpressionParserZero.Operands;
using FunctionZero.ExpressionParserZero.Tokens;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;


namespace FunctionZero.Maui.zBind.z
{
    internal class EvaluatorMultiConverter : IMultiValueConverter
    {
        private readonly VariableEvaluator _evaluator;
        private readonly ExpressionTree _compiledExpressionTree;
        private readonly List<ExpressionTreeNode>? _unExpressionTreeParentList;
        private readonly ExpressionTree? _unExpressionTree;
        private readonly string? _variableName;
        private readonly bool _isMultiBind;

        public EvaluatorMultiConverter(ICollection<string> keys, ExpressionTree compiledExpression, Bind bindingExtension)
        {
            if (bindingExtension is MultiBind)
                _isMultiBind = true;

            _evaluator = new VariableEvaluator(new List<string>(keys), bindingExtension);
            _compiledExpressionTree = compiledExpression;

            // If the ExpressionTree has a variable at the leaf, where all ancestors are a unary operator,
            // we can build an expression that does the opposite. Bail out if that's not possible.

            // This ExpressionTree will lie about its RpnTokens.
            var unExpressionTree = new ExpressionTree(new List<IToken>());
            List<ExpressionTreeNode> unExpressionTreeParentList = unExpressionTree.RootNodeList;
            var node = _compiledExpressionTree.RootNodeList[0];

            // For each unary operator in the _compiledExpressionTree, copy it to the unExpressionTree.
            while (node.Children.Count == 1)
            {
                unExpressionTreeParentList.Add(new ExpressionTreeNode(node.Token, 1));
                unExpressionTreeParentList = unExpressionTreeParentList[0].Children;
                node = node.Children[0];
            }
            if (node.Token is Operand operand)
            {
                if (operand.Type == OperandType.Variable)
                {
                    // Leaf node of original expression.
                    // We'll want to substitute this with an operand wrapping the new value.
                    _unExpressionTreeParentList = unExpressionTreeParentList;
                    // Placeholder
                    _unExpressionTreeParentList.Add(new ExpressionTreeNode(null, 0));
                    // node is the variable we want to assign.
                    _variableName = (string)operand.GetValue();
                    _unExpressionTree = unExpressionTree;
                }
            }
            else
            {
                // Non-unary operator. We cannot build the un-expression. Do nothing. 
            }
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (Array.Exists(values, value => ReferenceEquals(value, BindableProperty.UnsetValue)))
                return Binding.DoNothing;

            _evaluator.SetValues(values);

            try
            {
                var stack = _compiledExpressionTree.Evaluate(_evaluator);
                if (_isMultiBind)
                {
                    var retval = new List<object>();
                    while (stack.Count > 0)
                        retval.Add(ExpressionParserZero.OperatorActions.PopAndResolve(stack, _evaluator).GetValue());
                    return retval;
                }
                else
                {
                    var thing = ExpressionParserZero.OperatorActions.PopAndResolve(stack, _evaluator);
                    return thing.GetValue();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"z:Bind Convert failed: {ex.Message}");
                return Binding.DoNothing;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            try
            {
                if (_unExpressionTree != null && _unExpressionTreeParentList != null && _variableName != null)
                {
                    var propInfo = _evaluator.GetPropertyInfo(_variableName);

                    if (propInfo != null && propInfo.CanWrite)
                    {
                        if (BackingStoreHelpers.OperandTypeLookup.TryGetValue(propInfo.PropertyType, out var theOperandType))
                        {
                            var valueContainer = new Operand(theOperandType, value);
                            _unExpressionTreeParentList[0] = new ExpressionTreeNode(valueContainer, 0);
                            var result = _unExpressionTree.Evaluate(_evaluator);

                            _evaluator.SetValue(_variableName, result.Pop().GetValue());
                        }
                    }
                }
                else
                {
                    // Attempt to evaluate the expression to a writable reference (for indexed expressions)
                    var resultStack = _compiledExpressionTree.Evaluate(_evaluator);
                    if (resultStack.Count > 0)
                    {
                        var top = resultStack.Pop();
                        if (top is IWritableOperand writable)
                        {
                            // Let the indexed operand handle conversion and assignment
                            writable.SetValue(value);
                        }
                        else if (top is Operand op && op.Type == OperandType.Variable)
                        {
                            // Fallback: single variable assignment
                            var name = (string)op.GetValue();
                            var propInfo = _evaluator.GetPropertyInfo(name);
                            if (propInfo != null && propInfo.CanWrite)
                            {
                                _evaluator.SetValue(name, value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"z:Bind ConvertBack failed: {ex.Message}");
            }

            var sourceResults = new object[targetTypes.Length];
            Array.Fill(sourceResults, Binding.DoNothing);
            return sourceResults;
        }
    }
}