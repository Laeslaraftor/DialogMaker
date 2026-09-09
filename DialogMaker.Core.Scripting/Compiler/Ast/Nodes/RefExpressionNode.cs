using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents reference to variable or field
    /// </summary>
    /// <param name="token">Token that represents ref keyword</param>
    public class RefExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression of variable/field identifier or access to it
        /// </summary>
        public ExpressionNode? ReferencedExpression { get; set; }

        #region Static

        /// <summary>
        /// Parse reference expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed reference expression</returns>
        public static RefExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Ref);
            RefExpressionNode result = new(token)
            {
                ReferencedExpression = ParseExpression(stream)
            };
            result.ReferencedExpression.Parent = result;

            return result;
        }

        #endregion
    }
}
