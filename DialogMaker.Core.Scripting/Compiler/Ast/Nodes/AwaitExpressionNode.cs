using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Await node
    /// </summary>
    /// <param name="token">Token that represents await expression</param>
    public class AwaitExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression to await
        /// </summary>
        public ExpressionNode? Expression { get; set; }

        #region Статика

        /// <summary>
        /// Parse await expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed await expression</returns>
        public static AwaitExpressionNode Parse(AstParserStream stream)
        {
            var awaitToken = stream.Eat(DSharpTokenType.Await);
            var expression = UnaryExpressionNode.Parse(stream);

            return new AwaitExpressionNode(awaitToken)
            {
                Expression = expression,
            };
        }

        #endregion
    }
}
