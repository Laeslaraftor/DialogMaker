using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    public class UnaryExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        public DSharpUnaryOperator Operator { get; set; }
        public ExpressionNode? Operand { get; set; }

        #region Статика

        /// <summary>
        /// Check is current token unary operator
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Is current token unary operator</returns>
        public static bool IsUnaryOperator(AstParserStream stream)
        {
            var currentToken = stream.Current;

            if (currentToken == null)
            {
                return false;
            }

            var tokenType = currentToken.Type;

            return tokenType == DSharpTokenType.Increment ||
                   tokenType == DSharpTokenType.Decrement ||
                   tokenType == DSharpTokenType.Minus ||
                   tokenType == DSharpTokenType.Not;
        }

        /// <summary>
        /// Parse unary expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Parsed unary expression</returns>
        public static ExpressionNode Parse(AstParserStream stream)
        {
            if (stream.Current == null)
            {
                stream.ThrowPositionException("Invalid token");
            }

            if (stream.CheckAll<DSharpUnaryOperator>())
            {
                var op = stream.Eat(stream.Current.Type);
                var operand = Parse(stream);

                return new UnaryExpressionNode(op)
                {
                    Operator = (DSharpUnaryOperator)op.Type,
                    Operand = operand,
                };
            }

            if (stream.Check(DSharpTokenType.Await))
            {
                return AwaitExpressionNode.Parse(stream);
            }

            return ParsePrimary(stream);
        }

        #endregion
    }
}
