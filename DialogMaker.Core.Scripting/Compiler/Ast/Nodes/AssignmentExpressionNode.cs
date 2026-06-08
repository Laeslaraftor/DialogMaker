using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Assignment expression node
    /// </summary>
    /// <param name="token">Token that represents assignment operator</param>
    public class AssignmentExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Left expression of operation
        /// </summary>
        public ExpressionNode? Left { get; set; }
        /// <summary>
        /// Operator of this operation
        /// </summary>
        public DSharpAssignmentOperator Operator { get; set; }
        /// <summary>
        /// Right expression of operation
        /// </summary>
        public ExpressionNode? Right { get; set; }

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Left == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Operator: {((DSharpTokenType)Operator)}");
            builder.AppendLine($"Left: {Left}");

            if (Right != null)
            {
                builder.Append($"Right: {Right}");
            }

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Try to parse assignment expression node
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="result">Parsed assignment expression node</param>
        /// <returns>Returns true when assignment expression node was successfully parsed</returns>
        public static bool TryParse(AstParserStream stream, [NotNullWhen(true)] out AssignmentExpressionNode? result)
        {
            result = null;

            if (!stream.CheckAll<DSharpAssignmentOperator>())
            {
                return false;
            }

            var op = stream.Eat(stream.Current!.Type);
            var right = ParseExpression(stream);
            result = new(op)
            {
                Operator = (DSharpAssignmentOperator)op.Type,
                Right = right,
            };

            return true;
        }

        #endregion
    }
}
