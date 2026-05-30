using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics.CodeAnalysis;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents literal value like number, string, boolean or null
    /// </summary>
    /// <param name="token">Literal value token</param>
    public class LiteralExpressionNode(DialogScriptToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Value of this node
        /// </summary>
        public object? Value { get; set; }
        /// <summary>
        /// Type of literal value
        /// </summary>
        public DialogScriptLiteralType Type { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"Value: {Value ?? "null"}, type: {Type}. {base.ToString()}";
        }

        #endregion

        #region Статика

        private static readonly Dictionary<DialogScriptTokenType, LiteralInfo> _literals = new()
        {
            [DialogScriptTokenType.NumberLiteral] = new(DialogScriptLiteralType.Number, v => double.Parse(v.Replace(".", ","))),
            [DialogScriptTokenType.StringLiteral] = new(DialogScriptLiteralType.String, v => v),
            [DialogScriptTokenType.False] = new(DialogScriptLiteralType.Bool, v => false),
            [DialogScriptTokenType.True] = new(DialogScriptLiteralType.Bool, v => true),
            [DialogScriptTokenType.Null] = new(DialogScriptLiteralType.Null, v => null)
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
                    result = new(token)
                    {
                        Value = info.Value.Parser(token.Value),
                        Type = info.Value.Type,
                    };

                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Структуры

        private readonly struct LiteralInfo(DialogScriptLiteralType type, Func<string, object?> parser)
        {
            public DialogScriptLiteralType Type { get; } = type;
            public Func<string, object?> Parser { get; } = parser;
        }

        #endregion
    }
}
