using DialogMaker.Core.Scripting.Compiler.Lexer;
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
            return Position + offset < _lexer.Tokens.Count ? _lexer.Tokens[Position + offset] : null;
        }
        public bool IsEndOfFile() => Current?.Type == DSharpTokenType.EndOfFile;
        public bool Check(DSharpTokenType type, int offset = 0) => Peek(offset)?.Type == type;
        public bool Check(params DSharpTokenType[] types) => types.Contains(Current?.Type ?? DSharpTokenType.EndOfFile);
        public bool CheckAll<T>() where T : struct
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                if (Check((DSharpTokenType)value))
                {
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
            if (expected != null)
            {
                if (expected.Length == 1)
                {
                    throw new Exception($"Expected {expected[0]}, got {Current?.Type} at {Current?.Line}:{Current?.Column}");
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

                throw new Exception($"Expected {expectedTokens}, got {Current?.Type} at {Current?.Line}:{Current?.Column}");
            }

            throw new Exception($"Unexpected token {Current?.Type} at {Current?.Line}:{Current?.Column}");
        }
        [DoesNotReturn]
        public void ThrowPositionException(string message)
        {
            throw new Exception($"{message} at {Current?.Line}:{Current?.Column}");
        }
    }
}
