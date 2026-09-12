using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents unary expression
    /// </summary>
    /// <param name="token">Token that represents unary operator</param>
    public class UnaryExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Unary operation
        /// </summary>
        public DSharpUnaryOperator Operator { get; set; }
        /// <summary>
        /// Expression for performing unary operation
        /// </summary>
        public ExpressionNode? Operand { get; set; }

        #region Static

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

            if (stream.CheckAll(DSharpUnaryOperatorHelper.Values))
            {
                var operatorToken = stream.Eat(stream.Current.Type);
                var operand = Parse(stream);
                UnaryExpressionNode unaryExpression = new(operatorToken)
                {
                    Operator = (DSharpUnaryOperator)operatorToken.Type,
                    Operand = operand,
                };
                operand.Parent = unaryExpression;

                return unaryExpression;
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
