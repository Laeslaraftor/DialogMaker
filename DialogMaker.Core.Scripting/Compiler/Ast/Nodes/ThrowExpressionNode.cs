using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents throwing exception
    /// </summary>
    /// <param name="token">Token that represents throw keyword</param>
    public class ThrowExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression of throwing value
        /// </summary>
        public ExpressionNode? ValueExpression { get; set; }

        #region Статика

        /// <summary>
        /// Parse throw expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed throw expression</returns>
        public static ThrowExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Throw);

            return new(token)
            {
                ValueExpression = ParseExpression(stream)
            };
        }

        #endregion
    }
}
