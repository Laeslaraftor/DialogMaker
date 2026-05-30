using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Break node for stopping loop execution
    /// </summary>
    /// <param name="token">Token that represents break statement</param>
    public class BreakStatementNode(DialogScriptToken token) : StatementNode(token)
    {

        #region Статика

        /// <summary>
        /// Parse break statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed break statement</returns>
        public static BreakStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DialogScriptTokenType.Break);
            stream.Eat(DialogScriptTokenType.Semicolon);

            return new(token);
        }

        #endregion
    }
}
