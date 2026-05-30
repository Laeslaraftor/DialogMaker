using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement that references to struct
    /// </summary>
    /// <param name="token">Token that represents struct name</param>
    public class StructStatementNode(DialogScriptToken token) : StatementNode(token)
    {
        /// <summary>
        /// Referenced struct
        /// </summary>
        public StructNode? Struct { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return Struct?.ToString() ?? base.ToString();
        }

        #endregion
    }
}
