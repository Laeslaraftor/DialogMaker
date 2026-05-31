using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents decrement of value by 1
    /// </summary>
    /// <param name="token">Token that represents decrement operator</param>
    public class DecrementExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression that decrementing
        /// </summary>
        public ExpressionNode? Expression { get; set; }
    }
}
