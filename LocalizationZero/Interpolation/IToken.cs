using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalisationZero.Interpolation
{
    internal abstract class IToken
    {
        internal IToken(string content)
        {
            Content = content;
        }

        public string Content { get; }
    }

    internal class StringToken : IToken
    {
        public StringToken(string content) : base(content)
        {

        }
    }
    internal class ExpressionToken : IToken
    {
        public ExpressionToken(string content) : base(content)
        {

        }
    }
}
