using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement node that references to invokable node
    /// </summary>
    /// <param name="token">Token that represent invokable node</param>
    public class InvokableStatementNode(DialogScriptToken token) : StatementNode(token)
    {
        /// <summary>
        /// Referenced invokable node
        /// </summary>
        public InvokableNode? Invokable { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Invokable?.ToString() ?? base.ToString();
        }

        #endregion
    }
}
