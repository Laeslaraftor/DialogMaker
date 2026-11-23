using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public class AstParserStream(DSharpLexer lexer)
    {
        public DSharpToken? Current => Position < _lexer.Tokens.Count ? _lexer.Tokens[Position] : null;
        public int Position { get; set; }
        public int Line => Current != null ? Current.Line : 0;
        public int Column => Current != null ? Current.Column : 0;

        private readonly DSharpLexer _lexer = lexer;

        public DSharpToken? Peek(int offset = 1)
        {
            var position = Position + offset;

            if (0 > position || position >= _lexer.Tokens.Count)
            {
                return null;
            }

            return _lexer.Tokens[position];
        }
        public bool IsEndOfFile() => Current?.Type == DSharpTokenType.EndOfFile;
        public bool Check(DSharpTokenType type, int offset = 0)
        {
            var token = Peek(offset);
            return token?.Type == type;
        }
        public bool Check(params DSharpTokenType[] types) => types.Contains(Current?.Type ?? DSharpTokenType.EndOfFile);
        public bool CheckAll<T>(ImmutableArray<T> values) where T : struct
        {
            return CheckAll(values, out _);
        }
        public bool CheckAll<T>(ImmutableArray<T> values, out T passedToken) where T : struct
        {
            passedToken = default;

            foreach (var value in values)
            {
                var numberValue = (int)Convert.ChangeType(value, typeof(int));
                var typedValue = (DSharpTokenType)Enum.ToObject(typeof(DSharpTokenType), numberValue);

                if (Check(typedValue))
                {
                    passedToken = value;
                    return true;
                }
            }

            return false;
        }
        public DSharpToken Eat(DSharpTokenType expected)
        {
            if (Current?.Type == expected)
            {
                var token = Current;
                Position++;
                return token;
            }

            ThrowUnexpectedTokenException(expected);
            return null;
        }
        [DoesNotReturn]
        public void ThrowUnexpectedTokenException()
        {
            ThrowUnexpectedTokenException(null);
        }
        [DoesNotReturn]
        public void ThrowUnexpectedTokenException(params DSharpTokenType[]? expected)
        {
            var current = Current
                ?? throw new DSharpException($"Token{(expected != null && expected.Length > 0 ? " " + expected[0].ToString() : string.Empty)} was required but it not exists"); ;

            if (expected != null)
            {
                if (expected.Length == 1)
                {
                    throw new DSharpAstException($"Expected {expected[0]}, got {current.Type}", current);
                }

                string expectedTokens = string.Empty;

                foreach (var token in expected)
                {
                    if (!string.IsNullOrEmpty(expectedTokens))
                    {
                        expectedTokens += " or ";
                    }

                    expectedTokens += token;
                }

                throw new DSharpAstException($"Expected {expectedTokens}, got {current.Type}", current);
            }

            throw new DSharpAstException($"Unexpected token {current.Type}", current);
        }
        [DoesNotReturn]
        public void ThrowPositionException(string message)
        {
            var current = Current
                ?? throw new DSharpException(message);

            throw new DSharpAstException(message, current);
        }
    }
}
