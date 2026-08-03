using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents conditional expression (condition ? true : false)
    /// </summary>
    /// <param name="token">Token that represents question mark</param>
    public class ConditionalExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Condition of this expression
        /// </summary>
        public ExpressionNode? Condition { get; set; }
        /// <summary>
        /// Expression that executes when condition is <c>true</c>
        /// </summary>
        public ExpressionNode? TrueExpression { get; set; }
        /// <summary>
        /// Expression that executes when condition is <c>false</c>
        /// </summary>
        public ExpressionNode? FalseExpression { get; set; }

        #region Static

        /// <summary>
        /// Parse conditional expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed conditional expression</returns>
        public static ConditionalExpressionNode Parse(AstParserStream stream)
        {
            var question = stream.Eat(DSharpTokenType.Question);
            var trueExpression = ParseExpression(stream);
            stream.Eat(DSharpTokenType.Colon);
            var falseExpression = ParseExpression(stream);

            return new(question)
            {
                TrueExpression = trueExpression,
                FalseExpression = falseExpression
            };
        }

        #endregion
    }
}
