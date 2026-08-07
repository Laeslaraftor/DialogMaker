using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Expression node that contains expression in paren
    /// </summary>
    /// <param name="token">Token that represents open paren</param>
    public class ParenContainedExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Contained expression
        /// </summary>
        public ExpressionNode? Expression { get; set; }

        #region Controls

        /// <summary>
        /// Get content of paren contained expression
        /// </summary>
        /// <returns>Content of current expression</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ExpressionNode GetContent()
        {
            var expression = Expression;

            while (expression is ParenContainedExpressionNode parenContained)
            {
                expression = parenContained.Expression;
            }

            if (expression == null)
            {
                throw new InvalidOperationException($"Incomplete expression: {this}");
            }

            return expression;
        }

        #endregion

        #region Static

        /// <summary>
        /// Parse paren contained expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</</param>
        /// <returns>Paren contained expression</returns>
        public static ParenContainedExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.LeftParen);
            ParenContainedExpressionNode expression = new(token);

            do
            {
                expression.Expression = ParseExpression(stream);
            }
            while (expression.Expression is ParenContainedExpressionNode);

            stream.Eat(DSharpTokenType.RightParen);

            return expression;
        }

        #endregion
    }
}
