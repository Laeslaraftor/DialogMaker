using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Expression that means access to base type
    /// </summary>
    /// <param name="token">Token that represents base keyword</param>
    public class BaseExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        #region Статика

        /// <summary>
        /// Parse base expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed base expression</returns>
        public static BaseExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.Base);
            return new(token);
        }

        #endregion
    }
}
