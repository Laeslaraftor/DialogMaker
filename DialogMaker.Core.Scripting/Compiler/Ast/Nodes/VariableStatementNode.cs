using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement that references to variable
    /// </summary>
    /// <param name="token">Token that represents variable</param>
    public class VariableStatementNode(DialogScriptToken token) : StatementNode(token)
    {
        /// <summary>
        /// Variable node
        /// </summary>
        public VariableNode? Variable { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Variable?.ToString() ?? base.ToString();
        }

        #endregion
    }
}
