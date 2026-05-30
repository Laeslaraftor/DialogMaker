using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that references to enum node
    /// </summary>
    /// <param name="token">Token that represents enum name</param>
    public class EnumStatementNode(DialogScriptToken token) : StatementNode(token)
    {
        /// <summary>
        /// Referenced enum node
        /// </summary>
        public EnumNode? Enum { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Enum?.ToString() ?? base.ToString();
        }

        #endregion
    }
}
