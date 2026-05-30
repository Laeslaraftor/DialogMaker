using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Named node
    /// </summary>
    /// <param name="token">Token for naming node</param>
    public class NamedNode(DialogScriptToken token) : AstNode(token)
    {
        /// <summary>
        /// Token that represents this node
        /// </summary>
        public DialogScriptToken Token { get; } = token;
        /// <summary>
        /// Name of current node
        /// </summary>
        public string Name { get; set; } = token.Value;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"{GetType().Name}({Name}) at {base.ToString()}";
        }

        #endregion
    }
}
