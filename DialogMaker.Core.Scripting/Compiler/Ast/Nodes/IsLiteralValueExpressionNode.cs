using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents <c>is</c> expression with comparing value with literal value
    /// </summary>
    /// <param name="token">Token that represents <c>is</c> keyword</param>
    public class IsLiteralValueExpressionNode(DSharpToken token) : IsExpressionNode(token)
    {
        /// <summary>
        /// Literal value for comparing
        /// </summary>
        public LiteralExpressionNode? LiteralExpression { get; set; }

        public override string ToString()
        {
            if (LiteralExpression == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new(base.ToString());
            builder.AppendLine();
            builder.Append($"Literal expression: {LiteralExpression}");

            return builder.ToString();
        }
    }
}
