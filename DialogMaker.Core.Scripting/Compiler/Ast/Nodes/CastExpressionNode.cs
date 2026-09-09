using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents casting an expression
    /// </summary>
    /// <param name="token">Token that represents type identifier</param>
    public class CastExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Target type for cast
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Expression that should be casted
        /// </summary>
        public ExpressionNode? Expression { get; set; }

        #region Static

        /// <summary>
        /// Is cast expression now
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Return true if now cast expression</returns>
        public static bool IsCast(AstParserStream stream)
        {
            if (!TypeInfoNode.CanParse(stream, 1, out var endOffset))
            {
                return false;
            }

            return stream.Check(DSharpTokenType.RightParen, endOffset);
        }

        /// <summary>
        /// Parse cast expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed cast expression</returns>
        public static CastExpressionNode Parse(AstParserStream stream)
        {
            stream.Eat(DSharpTokenType.LeftParen);
            TypeInfoNode type = TypeInfoNode.Parse(stream, true, true);
            stream.Eat(DSharpTokenType.RightParen);
            CastExpressionNode result = new(type.Token)
            {
                Type = type,
                Expression = ParseExpression(stream)
            };

            type.Parent = result; ;
            result.Expression.Parent = result;

            return result;
        }

        #endregion
    }
}
