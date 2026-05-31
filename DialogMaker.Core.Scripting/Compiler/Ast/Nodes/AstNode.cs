using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Root node of abstract syntax tree
    /// </summary>
    /// <remarks>
    /// Create new instance of root node
    /// </remarks>
    /// <param name="token">Token that represents this node</param>
    public abstract class AstNode(DSharpToken token)
    {
        /// <summary>
        /// Token that represents this node
        /// </summary>
        public DSharpToken Token { get; } = token;
        /// <summary>
        /// Name of this node
        /// </summary>
        public string Name { get; } = token.Value;
        /// <summary>
        /// Line of this node in source code
        /// </summary>
        public int Line { get; set; } = token.Line;
        /// <summary>
        /// Column of this node is source code
        /// </summary>
        public int Column { get; set; } = token.Column;

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return $"{GetType().Name}({Name}) at line: {Line}, column: {Column}";
        }

        #endregion
    }
}
