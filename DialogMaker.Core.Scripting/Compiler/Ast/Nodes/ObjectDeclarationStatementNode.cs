using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement that references to struct
    /// </summary>
    /// <param name="token">Token that represents struct name</param>
    public class ObjectDeclarationStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Referenced struct
        /// </summary>
        public ObjectDeclarationNode? ObjectDeclaration { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            return ObjectDeclaration?.ToString() ?? base.ToString();
        }

        #endregion
    }
}
