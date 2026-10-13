using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;

namespace DialogMaker.Core.Scripting.Compiler.Analyzers
{
    /// <summary>
    /// Visitor that handles nodes visiting with provided delegate handler
    /// </summary>
    public class DSharpAstNodeDelegateVisitor : DSharpAstVisitor
    {
        /// <summary>
        /// Nodes handler
        /// </summary>
        public Action<AstNode>? Handler { get; set; }

        public override void StartVisiting(AstNode node)
        {
            base.StartVisiting(node);
            Handler?.Invoke(node);
        }
    }
}
