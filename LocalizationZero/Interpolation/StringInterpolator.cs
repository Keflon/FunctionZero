using FunctionZero.ExpressionParserZero;
using FunctionZero.ExpressionParserZero.BackingStore;
using FunctionZero.ExpressionParserZero.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalisationZero.Interpolation
{
    internal class StringInterpolator
    {
        public StringInterpolator(string input, IBackingStore backingStore, ExpressionParser parser)
        {
            var tokenizer = new Tokenizer(input);

            IToken token;

            var sb = new StringBuilder();

            while ((token = tokenizer.GetNextToken()) != null)
            {
                if(token is StringToken)
                    sb.Append(token.Content);
                else // ExpressionToken
                {
                    var tree = parser.Parse(token.Content);
                    var stack = tree.Evaluate(backingStore);
                    var result = OperatorActions.PopAndResolve(stack, backingStore);
                    sb.Append(result);
                }
            }

            Result = sb.ToString();
        }

        public string Result { get; }
    }
}
