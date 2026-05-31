using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Call expression
    /// </summary>
    /// <param name="token">Token that represents calling expression</param>
    public class CallExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression that calling
        /// </summary>
        public ExpressionNode? Callee { get; set; }
        /// <summary>
        /// Arguments of calling
        /// </summary>
        public List<ExpressionNode> Arguments { get; set; } = [];
    }
}
