using System.Diagnostics;

namespace LocalizationZero.Interpolation
{
    internal class Tokenizer
    {
        enum Chartype
        {
            Character = 0,
            OpenExpression,
            CloseExpression,
            End
        }

        int _length;
        int _cursor;

        private readonly string _input;

        public Tokenizer(string input)
        {
            _input = input;
            _length = _input.Length;
            _cursor = 0;
        }

        public IToken GetNextToken()
        {
            //Debug.WriteLine($"hello {{{5} {6} {7}}}");
            //Debug.WriteLine($"}   { 5 { hello } }");

            (Chartype type, char character) peekChar = PeekNextChar();

            switch (peekChar.type)
            {
                case Chartype.Character:
                    return ParseString();

                case Chartype.OpenExpression:
                    return ParseExpression();

                case Chartype.CloseExpression:
                    throw new InvalidOperationException($"Unexpected Close expression at {_cursor}");

                case Chartype.End:
                    return null;

            }
            return null;
        }

        private IToken ParseString()
        {
            string theString = string.Empty;

            while (true)
            {
                (Chartype type, char character) nextChar = PeekNextChar();

                switch (nextChar.type)
                {
                    case Chartype.Character:
                        theString += ReadNextChar().character;
                        break;
                    case Chartype.OpenExpression:
                        return new StringToken(theString);

                    case Chartype.CloseExpression:
                        throw new InvalidOperationException($"Unexpected '}}' at offset {_cursor}");

                    case Chartype.End:
                        return new StringToken(theString);
                }
            }
        }

        private IToken ParseExpression()
        {
            string theString = string.Empty;

            ReadNextChar(); // Skip the {

            while (true)
            {
                (Chartype type, char character) nextChar = PeekNextChar();

                switch (nextChar.type)
                {
                    case Chartype.Character:
                        if((nextChar.character == '{') || (nextChar.character == '}'))
                            throw new InvalidOperationException($"Unexpected '{nextChar.character}' at offset {_cursor}");

                        theString += ReadNextChar().character;
                        break;
                    case Chartype.OpenExpression:
                        throw new InvalidOperationException($"Unexpected '{{' at offset {_cursor}");

                    case Chartype.CloseExpression:
                        ReadNextChar(); // Skip the }
                        return new ExpressionToken(theString);

                    case Chartype.End:
                        throw new InvalidOperationException($"Missing '}}' at offset {_cursor}");
                }
            };
        }

        private (Chartype type, char character) ReadNextChar()
        {
            var retval = PeekNextChar();

            _cursor++;
            if (retval.type == Chartype.Character)
                if ((retval.character == '{') || (retval.character == '}'))
                    _cursor++;

            return retval;
        }

        private (Chartype type, char character) PeekNextChar()
        {
            if (_cursor == _length)
                return (Chartype.End, '\0');

            if (_input[_cursor] == '{')
                if (PeekNextNextChar() == '{')
                    return (Chartype.Character, '{');
                else
                    return (Chartype.OpenExpression, '{');

            if (_input[_cursor] == '}')
                if (PeekNextNextChar() == '}')
                    return (Chartype.Character, '}');
                else
                    return (Chartype.CloseExpression, '}');

            return (Chartype.Character, _input[_cursor]);
        }

        private char PeekNextNextChar()
        {
            if (_cursor+1 == _length)
                return '\0';

            return _input[_cursor + 1];
        }
    }
}
