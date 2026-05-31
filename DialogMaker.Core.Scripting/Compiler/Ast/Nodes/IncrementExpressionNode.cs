using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents increment of value by 1
    /// </summary>
    /// <param name="token">Token that represents increment operator</param>
    public class IncrementExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression that incrementing
        /// </summary>
        public ExpressionNode? Expression { get; set; }
    }
}
