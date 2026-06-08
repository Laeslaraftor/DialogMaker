using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

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

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Callee == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Callee: {Callee.ToString().Trim()}");

            if (Arguments.Count > 0)
            {
                builder.AppendLine("Arguments:");
                
                foreach (var arg in Arguments)
                {
                    builder.AppendLine(arg.ToString().Trim());
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
