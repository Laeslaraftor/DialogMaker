using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Namespace with block of statements
    /// </summary>
    /// <param name="token"><inheritdoc/></param>
    public class NamespaceBlockStatementBlock(DSharpToken token) : NamespaceStatementNode(token)
    {
        /// <summary>
        /// Block of statement that contains in this namespace
        /// </summary>
        public BlockStatementNode? Block { get; set; }
    }
}
