using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents literal value like number, string, boolean or null
    /// </summary>
    /// <param name="token">Literal value token</param>
    public class LiteralExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Value of this node
        /// </summary>
        public DSharpLiteralValue Value { get; set; }
        /// <summary>
        /// Type of literal value
        /// </summary>
        public DSharpLiteralType Type { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"Value: {Value}, type: {Type}. {base.ToString()}";
        }

        #endregion

        #region Статика

        private static readonly Dictionary<DSharpTokenType, Func<string, DSharpLiteralValue>> _literals = new()
        {
            [DSharpTokenType.NumberLiteral] = v => DSharpLiteralValue.Parse(v),
            [DSharpTokenType.StringLiteral] = v => v,
            [DSharpTokenType.CharLiteral] = v => v.Length == 0 ? '\0' : v[0],
            [DSharpTokenType.False] = v => false,
            [DSharpTokenType.True] = v => true,
            [DSharpTokenType.Null] = v => null
        };

        /// <summary>
        /// Parse literal value from current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed value from current token</returns>
        public static object? ParseValue(AstParserStream stream) => Parse(stream).Value;
        /// <summary>
        /// Parse literal node from current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed literal node from current token</returns>
        public static LiteralExpressionNode Parse(AstParserStream stream)
        {
            if (TryParse(stream, out var result))
            {
                return result;
            }

            stream.ThrowUnexpectedTokenException();

            return null;
        }
        /// <summary>
        /// Try parse literal node from current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="result">Parsed literal node from current token</param>
        /// <returns>Returns true when literal node successfully parsed</returns>
        public static bool TryParse(AstParserStream stream, [NotNullWhen(true)] out LiteralExpressionNode? result)
        {
            result = null;

            foreach (var info in _literals)
            {
                if (stream.Check(info.Key))
                {
                    var token = stream.Eat(info.Key);
                    var literalValue = info.Value(token.Value);
                    result = new(token)
                    {
                        Value = literalValue,
                        Type = literalValue.Type,
                    };

                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
