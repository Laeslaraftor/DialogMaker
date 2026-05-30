using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Ast
{
    public class AstParserStream(DialogScriptLexer lexer)
    {
        public DialogScriptToken? Current => Position < _lexer.Tokens.Count ? _lexer.Tokens[Position] : null;
        public int Position { get; set; }

        private readonly DialogScriptLexer _lexer = lexer;

        public DialogScriptToken? Peek(int offset = 1)
        {
            return Position + offset < _lexer.Tokens.Count ? _lexer.Tokens[Position + offset] : null;
        }
        public bool IsEndOfFile() => Current?.Type == DialogScriptTokenType.EndOfFile;
        public bool Check(DialogScriptTokenType type, int offset = 0) => Peek(offset)?.Type == type;
        public bool Check(params DialogScriptTokenType[] types) => types.Contains(Current?.Type ?? DialogScriptTokenType.EndOfFile);
        public bool CheckAll<T>() where T : struct
        {
            foreach (var value in Enum.GetValues(typeof(T)))
            {
                if (Check((DialogScriptTokenType)value))
                {
                    return true;
                }
            }

            return false;
        }
        public DialogScriptToken Eat(DialogScriptTokenType expected)
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
        public void ThrowUnexpectedTokenException(params DialogScriptTokenType[]? expected)
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
