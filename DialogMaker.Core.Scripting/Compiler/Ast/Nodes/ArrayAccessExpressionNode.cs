using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represent access to array item
    /// </summary>
    /// <param name="token">Token that represents index</param>
    public class ArrayAccessExpressionNode(DialogScriptToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Array expression that stores item
        /// </summary>
        public ExpressionNode? Array { get; set; }
        /// <summary>
        /// Index expression
        /// </summary>
        public ExpressionNode? Index { get; set; }

        #region Управление

        public override string ToString()
        {
            return $"Array access: {Array}[{Index}] at {base.ToString()}";
        }

        #endregion
    }
}
