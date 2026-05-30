using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Type info with access to member of other type
    /// </summary>
    /// <param name="token">Token that represents type</param>
    public class MemberTypeInfoNode(DialogScriptToken token) : TypeInfoNode(token)
    {
        /// <summary>
        /// Member access expression to current type
        /// </summary>
        public MemberAccessExpressionNode? Member { get; set; }
    }
}
