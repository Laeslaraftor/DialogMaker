using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Statement that references to expression
    /// </summary>
    /// <param name="token">Token that represents expression</param>
    public class ExpressionStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Referenced expression
        /// </summary>
        public ExpressionNode? Expression { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Expression == null)
            {
                return base.ToString();
            }

            return $"Expression statement: {Expression}";
        }

        #endregion
    }
}
