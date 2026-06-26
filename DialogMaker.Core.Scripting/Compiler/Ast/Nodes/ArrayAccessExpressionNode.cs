using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represent access to array item
    /// </summary>
    /// <param name="token">Token that represents index</param>
    public class ArrayAccessExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Array expression that stores item
        /// </summary>
        public ExpressionNode? Array { get; set; }
        /// <summary>
        /// Index expression
        /// </summary>
        public List<ExpressionNode> Arguments { get; set; } = [];

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Array == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine(Array.ToString());
            
            if (Arguments.Count > 0)
            {
                builder.AppendLine("Arguments:");
                int i = 0;

                foreach (var arg in Arguments)
                {
                    builder.AppendLine($"{i}: {arg}");
                    i++;
                }
            }            

            return builder.ToString().TrimEnd();
        }

        #endregion
    }
}
