using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Expression that represents accessing to current instance
    /// </summary>
    /// <param name="token">Token that represents this keyword</param>
    public class ThisExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        #region Статика

        /// <summary>
        /// Parse this expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed this expression</returns>
        public static ThisExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.This);
            return new(token);
        }

        #endregion
    }
}
