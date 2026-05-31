using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Member access expression
    /// </summary>
    /// <param name="token">Token that represents access operation</param>
    public class MemberAccessExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression of target
        /// </summary>
        public ExpressionNode? Target { get; set; }
        /// <summary>
        /// Accessed member
        /// </summary>
        public ExpressionNode? Member { get; set; }
    }
}
