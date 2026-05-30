using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Continue statement for skipping loop iteration
    /// </summary>
    /// <param name="token">Token that represents continue statement</param>
    public class ContinueStatementNode(DialogScriptToken token) : StatementNode(token)
    {

        #region Статика

        /// <summary>
        /// Parse continue statement starts from current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed continue statement</returns>
        public static ContinueStatementNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DialogScriptTokenType.Continue);
            stream.Eat(DialogScriptTokenType.Semicolon);

            return new(token);
        }

        #endregion
    }
}
