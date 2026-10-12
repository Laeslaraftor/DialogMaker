namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Interface of case ast node
    /// </summary>
    public interface ICaseNode
    {
        /// <summary>
        /// Is default case. Default cases does not have key expression
        /// </summary>
        public bool IsDefault { get; }
        /// <summary>
        /// Expression of comparing value
        /// </summary>
        public ExpressionNode? KeyExpression { get; }
        /// <summary>
        /// Node that represents value expression or body
        /// </summary>
        public AstNode? ValueNode { get; }

        /// <summary>
        /// Get current case as ast node
        /// </summary>
        /// <returns>Case ast node</returns>
        public AstNode AsNode();
    }
}
