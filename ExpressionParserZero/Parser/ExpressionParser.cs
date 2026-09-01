#region License
// Author: Keith Pickford
// 
// MIT License
// 
// Copyright (c) 2016 -2020 FunctionZero Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Evaluator;
using FunctionZero.ExpressionParserZero.Exceptions;
using FunctionZero.ExpressionParserZero.FunctionMatrices;
using FunctionZero.ExpressionParserZero.Operands;
using FunctionZero.ExpressionParserZero.Operators;
using FunctionZero.ExpressionParserZero.Parser.FunctionMatrices;
using FunctionZero.ExpressionParserZero.Parser.FunctionVectors;
using FunctionZero.ExpressionParserZero.Tokens;

//using Windows.Storage.Streams;

namespace FunctionZero.ExpressionParserZero.Parser
{
    public class ExpressionParser
    {
        enum State
        {
            None = 0,
            Operand,
            Operator,
            UnaryOperator,
            FunctionOperator,
            OpenParenthesis,
            CloseParenthesis,
            OpenBracket,
            CloseBracket,
            UnaryCastOperator
        }

        private sealed class ParseContext
        {
            public State State { get; set; }
            public Stack<OperatorWrapper> OperatorStack { get; } = new Stack<OperatorWrapper>();
            public Stack<DelimiterContext> Delimiters { get; } = new Stack<DelimiterContext>();
            public TokenList Tokens { get; } = new TokenList();
        }

        private enum DelimiterKind
        {
            Group,
            Function,
            Index
        }

        private sealed class DelimiterContext
        {
            public DelimiterContext(DelimiterKind kind)
            {
                Kind = kind;
            }

            public DelimiterKind Kind { get; }
            public int SeparatorCount { get; set; }
        }

        public const int FunctionPrecedence = 13;

        public ExpressionParser()
        {
            Operators = new Dictionary<string, IOperator>();
            OperatorVectors = new Dictionary<IOperator, SingleOperandFunctionVector>();
            OperatorMatrices = new Dictionary<IOperator, DoubleOperandFunctionMatrix>();
            Functions = new Dictionary<string, IOperator>();

            RegisterBuiltInCasts();
            RegisterBuiltInOperators();
            RegisterGrammarTokens();
            RegisterFunction("_debug_mul", OperatorActions.DoMultiply, 2);
        }

        private void RegisterBuiltInCasts()
        {
            var castMatrix = UnaryCastMatrix.Create();
            RegisterUnaryCastOperator(OperandType.Sbyte, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Byte, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Short, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Ushort, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Int, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Uint, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Long, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Ulong, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Char, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Float, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Double, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Bool, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Decimal, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableSbyte, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableByte, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableShort, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableUshort, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableInt, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableUint, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableLong, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableUlong, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableChar, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableFloat, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableDouble, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableBool, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.NullableDecimal, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.String, 12, castMatrix);
            RegisterUnaryCastOperator(OperandType.Object, 12, castMatrix);
        }

        private void RegisterBuiltInOperators()
        {
            UnaryMinus = RegisterUnaryOperator("UnaryMinus", 12, UnaryMinusVector.Create());
            UnaryPlus = RegisterUnaryOperator("UnaryPlus", 12, AddVector.Create());
            RegisterUnaryOperator("!", 12, UnaryNotVector.Create());
            RegisterUnaryOperator("~", 12, UnaryComplementVector.Create());
            RegisterOperator("*", 11, MultiplyMatrix.Create());
            RegisterOperator("/", 11, DivideMatrix.Create());
            RegisterOperator("%", 11, ModuloMatrix.Create());
            PlusOperator = RegisterOperator("+", 10, AddMatrix.Create());
            MinusOperator = RegisterOperator("-", 10, SubtractMatrix.Create());
            RegisterOperator("<", 9, LessThanMatrix.Create());
            RegisterOperator(">", 9, GreaterThanMatrix.Create());
            RegisterOperator(">=", 9, GreaterThanOrEqualMatrix.Create());
            RegisterOperator("<=", 9, LessThanOrEqualMatrix.Create());
            RegisterOperator("!=", 8, NotEqualMatrix.Create());
            RegisterOperator("==", 8, EqualityMatrix.Create());
            RegisterOperator("&", 7, BitwiseAndMatrix.Create());
            RegisterOperator("^", 6, BitwiseXorMatrix.Create());
            RegisterOperator("|", 5, BitwiseOrMatrix.Create());
            RegisterOperator("&&", 4, LogicalAndMatrix.Create(), ShortCircuitMode.LogicalAnd);
            RegisterOperator("||", 3, LogicalOrMatrix.Create(), ShortCircuitMode.LogicalOr);
            RegisterSetEqualsOperator("=", 2, SetEqualsMatrix.Create());
        }

        private void RegisterGrammarTokens()
        {
            CommaOperator = RegisterGrammarToken(",", 1, OperatorType.Operator);
            OpenParenthesisOperator = RegisterGrammarToken("(", 0, OperatorType.OpenParenthesis);
            CloseParenthesisOperator = RegisterGrammarToken(")", FunctionPrecedence, OperatorType.CloseParenthesis);
            OpenBracketOperator = RegisterGrammarToken("[", 0, OperatorType.OpenBracket);
            CloseBracketOperator = RegisterGrammarToken("]", FunctionPrecedence, OperatorType.CloseBracket);
        }

        private Dictionary<string, IOperator> Operators { get; }
        private Dictionary<IOperator, DoubleOperandFunctionMatrix> OperatorMatrices { get; }
        private Dictionary<IOperator, SingleOperandFunctionVector> OperatorVectors { get; }
        private Dictionary<string, IOperator> Functions { get; }

        private IOperator UnaryMinus { get; set; }
        private IOperator UnaryPlus { get; set; }
        private IOperator PlusOperator { get; set; }
        private IOperator MinusOperator { get; set; }
        private IOperator CommaOperator { get; set; }
        private IOperator OpenParenthesisOperator { get; set; }
        private IOperator CloseParenthesisOperator { get; set; }
        private IOperator OpenBracketOperator { get; set; }
        private IOperator CloseBracketOperator { get; set; }

        private IOperator RegisterGrammarToken(string text, int precedence, OperatorType operatorType)
        {
            var op = new Operator(operatorType, precedence, ShortCircuitMode.None, null, text);
            Operators.Add(text, op);
            return op;
        }

        public IOperator RegisterOperator(
            string text, 
            int precedence, 
            DoubleOperandFunctionMatrix matrix, 
            ShortCircuitMode shortCircuit = ShortCircuitMode.None, 
            OperatorType operatorType = OperatorType.Operator)
        {
            var op = new Operator(operatorType,
                precedence,
                shortCircuit,
                (operandStack, vSet, parserPosition) =>
                {
                    var result = OperatorActions.DoOperation(matrix, operandStack, vSet);
                    if (result != null)
                        throw new ExpressionEvaluatorException(
                            parserPosition,
                            ExpressionEvaluatorException.ExceptionCause.BadOperand,
                            "Operator '" + text + "' cannot be applied to operands of type " + result.Item1 + " and " + result.Item2);
                }, text
            );
            Operators.Add(text, op);
            OperatorMatrices.Add(op, matrix);
            return op;
        }

        public IOperator GetNamedOperator(string strName)
        {
            return Operators[strName];
        }

        public IOperator RegisterSetEqualsOperator(string text, int precedence, DoubleOperandFunctionMatrix matrix)
        {
            var op = new Operator(OperatorType.Operator, precedence, ShortCircuitMode.None,

                        (operandStack, vSet, parserPosition) =>
                        {
                            OperatorActions.DoSetEquals(matrix, operandStack, vSet, parserPosition);
                        }


                , text);
            Operators.Add(text, op);
            OperatorMatrices.Add(op, matrix);
            return op;
        }

        public IOperator RegisterUnaryOperator(string text, int precedence, SingleOperandFunctionVector vector)
        {
            var op = new Operator(
                OperatorType.UnaryOperator,
                precedence,
                ShortCircuitMode.None,
                (operandStack, vSet, parserPosition) =>
                {

                    var result = OperatorActions.DoUnaryOperation(vector, operandStack, vSet);
                    if (result != null)
                    {
                        throw new ExpressionEvaluatorException(parserPosition,
                            ExpressionEvaluatorException.ExceptionCause.BadUnaryOperand,
                            "Unary operator '" + text + "' cannot be applied to operand of type " + result.Item1);
                    }
                },
                text
            );
            Operators.Add(text, op);
            OperatorVectors.Add(op, vector);
            return op;
        }


        public IOperator RegisterUnaryCastOperator(OperandType operandType, int precedence, DoubleOperandFunctionMatrix matrix)
        {
            var text = operandType.ToString();

            var castToOperand = new Operand(operandType);

            var op = new Operator(
                OperatorType.UnaryCastOperator,
                precedence,
                ShortCircuitMode.None,
                (operandStack, vSet, parserPosition) =>
                {
                    var result = OperatorActions.DoUnaryCastOperation(matrix, operandStack, vSet, castToOperand);
                    if (result != null)
                    {
                        throw new ExpressionEvaluatorException(parserPosition,
                            ExpressionEvaluatorException.ExceptionCause.BadUnaryOperand,
                            "Cast to (" + text + ") cannot be applied to operand of type " + result.Item1);
                    }
                },
                text
            );
            Operators.Add(text, op);
            OperatorMatrices.Add(op, matrix);
            return op;
        }

        public IFunctionOperator RegisterFunction(string text, Action<Stack<IOperand>, IBackingStore, long> doOperation,
            int parameterCount, int maxParameterCount = 0)
        {
            var op = new FunctionOperator(OperatorType.Function, doOperation, text, parameterCount, maxParameterCount);
            Functions.Add(text, op);
            return op;
        }

        public ExpressionTree Parse(string expression)
        {
            return Parse(new MemoryStream(Encoding.UTF8.GetBytes(expression ?? "")));
        }

        public ExpressionTree Parse(Stream inputStream)
        {
            var context = new ParseContext();
            var tokenizer = new Tokenizer(inputStream, Operators, Functions);
            IToken token;
            while ((token = tokenizer.GetNextToken()) != null)
            {
                if (token is IOperator)
                    token = new OperatorWrapper(TranslateOperator((IOperator)token, context.State), tokenizer.Anchor);

                var operatorWrapper = token as OperatorWrapper;

                if (TryProcessGrammarToken(context, operatorWrapper, token))
                    continue;

                ValidateNextToken(token, context.State, HasOpenParenthesis(context));

                var previousState = context.State;
                context.State = GetState(token, context.State);

                RemoveCastParenthesis(context);
                ProcessToken(context, token, operatorWrapper, previousState);
            }

            if (context.State == State.Operator || context.State == State.UnaryOperator)
                throw new ExpressionParserException(tokenizer.ParserPosition,
                    ExpressionParserException.ExceptionCause.OperandExpected);
            if (context.State == State.FunctionOperator)
                throw new ExpressionParserException(tokenizer.ParserPosition,
                    ExpressionParserException.ExceptionCause.OpenParenthesisExpected);
            if (context.Delimiters.Count != 0)
            {
                var cause = context.Delimiters.Peek().Kind == DelimiterKind.Index
                    ? ExpressionParserException.ExceptionCause.ClosingBracketExpected
                    : ExpressionParserException.ExceptionCause.ClosingBraceExpected;
                throw new ExpressionParserException(tokenizer.ParserPosition, cause);
            }

            PopByPrecedence(context.OperatorStack, context.Tokens, 0);


            return new ExpressionTree(context.Tokens);
        }

        private bool TryProcessGrammarToken(ParseContext context, OperatorWrapper operatorWrapper, IToken token)
        {
            if (operatorWrapper?.WrappedOperator == OpenBracketOperator)
            {
                if (!IsValueState(context.State))
                    throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedOpenBracket);

                context.Delimiters.Push(new DelimiterContext(DelimiterKind.Index));
                context.OperatorStack.Push(operatorWrapper);
                context.State = State.OpenBracket;
                return true;
            }

            if (operatorWrapper?.WrappedOperator == CloseBracketOperator)
            {
                if (context.Delimiters.Count == 0 || context.Delimiters.Peek().Kind != DelimiterKind.Index)
                    throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnmatchedClosingBracket);
                if (!IsValueState(context.State))
                    throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedCloseBracket);

                PopUntilOpenBracket(context.OperatorStack, context.Tokens, token.ParserPosition);
                var indexCount = context.Delimiters.Pop().SeparatorCount + 1;
                context.Tokens.Add(new OperatorWrapper(new IndexOperator(indexCount), token.ParserPosition));
                context.State = State.CloseBracket;
                return true;
            }

            if (operatorWrapper?.WrappedOperator != CommaOperator || context.Delimiters.Count == 0)
                return false;

            var delimiter = context.Delimiters.Peek();
            if (delimiter.Kind == DelimiterKind.Index)
            {
                if (!IsValueState(context.State))
                    throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.MisplacedComma);

                PopToOpenBracket(context.OperatorStack, context.Tokens);
                delimiter.SeparatorCount++;
                context.State = State.Operator;
                return true;
            }

            if (delimiter.Kind == DelimiterKind.Function)
                delimiter.SeparatorCount++;

            return false;
        }

        private static bool IsValueState(State state)
        {
            return state == State.Operand || state == State.CloseParenthesis || state == State.CloseBracket;
        }

        private static void RemoveCastParenthesis(ParseContext context)
        {
            if (context.State != State.UnaryCastOperator)
                return;

            var openParenthesis = context.OperatorStack.Pop();
            if (openParenthesis.Type != OperatorType.OpenParenthesis)
                throw new InvalidOperationException();
        }

        private void ProcessToken(ParseContext context, IToken token, OperatorWrapper operatorWrapper, State previousState)
        {
            Debug.Assert(token is Operand || token is OperatorWrapper);

            if (token.TokenType == TokenType.Operand)
            {
                context.Tokens.Add(token);
                return;
            }

            ProcessOperator(context, operatorWrapper, previousState);
        }

        private void ProcessOperator(ParseContext context, OperatorWrapper operatorWrapper, State previousState)
        {
            switch (operatorWrapper.Type)
            {
                case OperatorType.Operator:
                    PopByPrecedence(context.OperatorStack, context.Tokens, operatorWrapper.Precedence);
                    if (operatorWrapper.WrappedOperator != CommaOperator)
                        context.OperatorStack.Push(operatorWrapper);
                    context.State = State.Operator;
                    break;
                case OperatorType.UnaryOperator:
                case OperatorType.Function:
                case OperatorType.UnaryCastOperator:
                    context.OperatorStack.Push(operatorWrapper);
                    break;
                case OperatorType.OpenParenthesis:
                    ProcessOpenParenthesis(context, operatorWrapper, previousState);
                    break;
                case OperatorType.CloseParenthesis:
                    ProcessCloseParenthesis(context, operatorWrapper.ParserPosition, previousState);
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected operator type '{operatorWrapper.Type}'.");
            }
        }

        private static void ProcessOpenParenthesis(ParseContext context, OperatorWrapper openParenthesis, State previousState)
        {
            var functionOperator = previousState == State.FunctionOperator ? context.OperatorStack.Peek() : null;
            context.Delimiters.Push(new DelimiterContext(
                functionOperator == null ? DelimiterKind.Group : DelimiterKind.Function));
            context.OperatorStack.Push(openParenthesis);
        }

        private static void ProcessCloseParenthesis(ParseContext context, long parserPosition, State previousState)
        {
            if (context.Delimiters.Count == 0 || context.Delimiters.Peek().Kind == DelimiterKind.Index)
                throw new ExpressionParserException(parserPosition, ExpressionParserException.ExceptionCause.UnmatchedClosingBrace);

            var delimiter = context.Delimiters.Pop();
            if (previousState == State.UnaryCastOperator)
                return;

            PopByPrecedence(context.OperatorStack, context.Tokens, 1);
            context.OperatorStack.Pop();

            if (delimiter.Kind != DelimiterKind.Function)
                return;

            var registeredFunction = context.OperatorStack.Pop();
            var actualParameterCount = previousState == State.OpenParenthesis ? 0 : delimiter.SeparatorCount + 1;
            context.OperatorStack.Push(CreateFunctionInvocation(registeredFunction, actualParameterCount));
        }

        private static OperatorWrapper CreateFunctionInvocation(OperatorWrapper registeredFunction, int actualParameterCount)
        {
            var definition = (IFunctionOperator)registeredFunction.WrappedOperator;
            if (actualParameterCount < definition.MinParameterCount || actualParameterCount > definition.MaxParameterCount)
            {
                throw new ExpressionParserException(
                    registeredFunction.ParserPosition,
                    ExpressionParserException.ExceptionCause.WrongNumberOfFunctionParameters,
                    $"Function '{definition.AsString}' expects {FormatParameterRange(definition)}, but received {actualParameterCount}.");
            }

            return new OperatorWrapper(
                new FunctionInvocationOperator(definition, actualParameterCount),
                registeredFunction.ParserPosition);
        }

        private static string FormatParameterRange(IFunctionOperator definition)
        {
            if (definition.MinParameterCount == definition.MaxParameterCount)
                return definition.MinParameterCount == 1 ? "1 argument" : $"{definition.MinParameterCount} arguments";

            return $"between {definition.MinParameterCount} and {definition.MaxParameterCount} arguments";
        }

        /// <summary>
        /// Depending on the current parser state, a + or - operator might need to be translated to a unary + or -
        /// </summary>
        /// <param name="op"></param>
        /// <returns></returns>
        private IOperator TranslateOperator(IOperator op, State state)
        {
            if ((op == MinusOperator) &&
                ((state == State.Operator) || (state == State.UnaryOperator) || (state == State.None) ||
                  (state == State.OpenParenthesis) || (state == State.OpenBracket)))
            {
                return UnaryMinus;
            }
            else if ((op == PlusOperator) &&
                      ((state == State.Operator) || (state == State.UnaryOperator) || (state == State.None) ||
                       (state == State.OpenParenthesis) || (state == State.OpenBracket)))
            {
                return UnaryPlus;
            }
            else
            {
                return op;
            }
        }

        private void ValidateNextToken(IToken token, State state, bool hasOpenParenthesis)
        {
            if (token == null)
                throw new ExpressionParserException(-1, ExpressionParserException.ExceptionCause.UnexpectedEndOfStream);

            if (token.TokenType == TokenType.Operand)
            {
                ValidateOperand(token, state);
                return;
            }

            var operatorType = ((IOperator)token).Type;
            switch (operatorType)
            {
                case OperatorType.Operator:
                    ValidateBinaryOperator(token, state);
                    break;
                case OperatorType.UnaryOperator:
                    ValidateExpressionStarter(token, state, ExpressionParserException.ExceptionCause.UnexpectedUnaryOperator);
                    break;
                case OperatorType.Function:
                    ValidateExpressionStarter(token, state, ExpressionParserException.ExceptionCause.UnexpectedFunctionCall);
                    break;
                case OperatorType.OpenParenthesis:
                    ValidateOpenParenthesis(token, state);
                    break;
                case OperatorType.CloseParenthesis:
                    ValidateCloseParenthesis(token, state, hasOpenParenthesis);
                    break;
                case OperatorType.UnaryCastOperator:
                    if (state != State.OpenParenthesis && state != State.OpenBracket)
                        throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedCastOperand);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operatorType));
            }
        }

        private static void ValidateOperand(IToken token, State state)
        {
            if (state == State.FunctionOperator)
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.OpenParenthesisExpected);
            if (!CanStartExpression(state))
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedOperand);
        }

        private static void ValidateBinaryOperator(IToken token, State state)
        {
            if (state == State.FunctionOperator)
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.OpenParenthesisExpected);
            if (!IsValueState(state))
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedOperator);
        }

        private static void ValidateExpressionStarter(IToken token, State state, ExpressionParserException.ExceptionCause cause)
        {
            if (state == State.FunctionOperator)
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.OpenParenthesisExpected);
            if (!CanStartExpression(state))
                throw new ExpressionParserException(token.ParserPosition, cause);
        }

        private static void ValidateOpenParenthesis(IToken token, State state)
        {
            if (state == State.Operand || state == State.CloseParenthesis || state == State.CloseBracket || state == State.UnaryCastOperator)
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedOpenParenthesis);
        }

        private static void ValidateCloseParenthesis(IToken token, State state, bool hasOpenParenthesis)
        {
            if (!hasOpenParenthesis)
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnmatchedClosingBrace);

            if (state != State.Operand &&
                state != State.OpenParenthesis &&
                state != State.CloseParenthesis &&
                state != State.CloseBracket &&
                state != State.UnaryCastOperator)
            {
                throw new ExpressionParserException(token.ParserPosition, ExpressionParserException.ExceptionCause.UnexpectedCloseParenthesis);
            }
        }

        private static bool CanStartExpression(State state)
        {
            return state == State.None ||
                   state == State.Operator ||
                   state == State.UnaryOperator ||
                   state == State.OpenParenthesis ||
                   state == State.OpenBracket;
        }

        private static State GetState(IToken token, State currentState)
        {
            if (token.TokenType == TokenType.Operand)
                return State.Operand;

            switch (((IOperator)token).Type)
            {
                case OperatorType.Operator:
                    return State.Operator;
                case OperatorType.UnaryOperator:
                    return State.UnaryOperator;
                case OperatorType.Function:
                    return State.FunctionOperator;
                case OperatorType.OpenParenthesis:
                    return State.OpenParenthesis;
                case OperatorType.CloseParenthesis:
                    return currentState == State.UnaryCastOperator ? State.None : State.CloseParenthesis;
                case OperatorType.OpenBracket:
                    return State.OpenBracket;
                case OperatorType.CloseBracket:
                case OperatorType.Index:
                    return State.CloseBracket;
                case OperatorType.UnaryCastOperator:
                    return State.UnaryCastOperator;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static void PopByPrecedence(Stack<OperatorWrapper> operatorStack, IList<IToken> tokenList,
            int currentPrecedence)
        {
            while (operatorStack.Count > 0 && operatorStack.Peek().WrappedOperator.Precedence >= currentPrecedence)
            {
                tokenList.Add(operatorStack.Pop());
            }
        }

        private static bool HasOpenParenthesis(ParseContext context)
        {
            foreach (var delimiter in context.Delimiters)
            {
                if (delimiter.Kind != DelimiterKind.Index)
                    return true;
            }

            return false;
        }

        private static void PopToOpenBracket(Stack<OperatorWrapper> operatorStack, IList<IToken> tokenList)
        {
            while (operatorStack.Count > 0 && operatorStack.Peek().Type != OperatorType.OpenBracket)
                tokenList.Add(operatorStack.Pop());
        }

        private static void PopUntilOpenBracket(Stack<OperatorWrapper> operatorStack, IList<IToken> tokenList, long parserPosition)
        {
            while (operatorStack.Count > 0 && operatorStack.Peek().Type != OperatorType.OpenBracket)
            {
                if (operatorStack.Peek().Type == OperatorType.OpenParenthesis)
                    throw new ExpressionParserException(parserPosition, ExpressionParserException.ExceptionCause.UnexpectedCloseBracket);

                tokenList.Add(operatorStack.Pop());
            }

            if (operatorStack.Count == 0)
                throw new ExpressionParserException(parserPosition, ExpressionParserException.ExceptionCause.UnmatchedClosingBracket);

            operatorStack.Pop();
        }

        public void RegisterOverload(string operatorName, OperandType left, OperandType right, DoubleOperandDelegate func)
        {
            if (Operators.TryGetValue(operatorName, out IOperator op))
            {
                if ((op.Type == OperatorType.Operator) || (op.Type == OperatorType.UnaryCastOperator))
                    OperatorMatrices[op].RegisterDelegate(left, right, func);
                else
                    throw new ExpressionParserException(-1, ExpressionParserException.ExceptionCause.DoubleOperandOperatorNotFound,
                        "'" + operatorName + "' is an operator or a unary-cast operator");
            }
            else
                throw new ExpressionParserException(-1, ExpressionParserException.ExceptionCause.DoubleOperandOperatorNotFound,
                    "'" + operatorName + "' is not a registered operator");
        }

        public void RegisterOverload(string operatorName, OperandType type, SingleOperandDelegate func)
        {
            if (Operators.TryGetValue(operatorName, out IOperator op))
            {
                if (op.Type == OperatorType.UnaryOperator)
                    OperatorVectors[op].RegisterDelegate(type, func);
                else
                    throw new ExpressionParserException(-1, ExpressionParserException.ExceptionCause.UnaryOperatorNotFound,
                        "'" + operatorName + "' is not a unary operator");
            }
            else
                throw new ExpressionParserException(-1, ExpressionParserException.ExceptionCause.UnaryOperatorNotFound,
                    "'" + operatorName + "' is not a registered operator");
        }
    }
}