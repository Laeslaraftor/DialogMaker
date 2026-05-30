using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Root node of abstract syntax tree
    /// </summary>
    public abstract class AstNode()
    {
        /// <summary>
        /// Create new instance of root node
        /// </summary>
        /// <param name="token">Token that represents this node</param>
        protected AstNode(DialogScriptToken token) : this()
        {
            Line = token.Line;
            Column = token.Column;
        }

        /// <summary>
        /// Line of this node in source code
        /// </summary>
        public int Line { get; set; }
        /// <summary>
        /// Column of this node is source code
        /// </summary>
        public int Column { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"line: {Line}, column: {Column}";
        }

        #endregion
    }
}
