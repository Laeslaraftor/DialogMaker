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
        /// Nodes start visiting handler
        /// </summary>
        public Action<AstNode>? StartHandler { get; set; }
        /// <summary>
        /// Nodes end visiting handler
        /// </summary>
        public Action<AstNode>? EndHandler { get; set; }

        public override void StartVisiting(AstNode node)
        {
            base.StartVisiting(node);
            StartHandler?.Invoke(node);
        }
        public override void EndVisiting(AstNode node)
        {
            base.EndVisiting(node);
            EndHandler?.Invoke(node);
        }
    }
}
