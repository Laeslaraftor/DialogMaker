using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents <c>is</c> expression with checking value type or/and fields
    /// </summary>
    /// <param name="token">Token that represents <c>is</c> keyword</param>
    public class IsTypeExpressionNode(DSharpToken token) : IsExpressionNode(token)
    {
        /// <summary>
        /// Type for checking expression
        /// </summary>
        public TypeInfoNode? DestinationType { get; set; }
        /// <summary>
        /// Identifier of variable that should contains checked value
        /// </summary>
        public IdentifierExpressionNode? DestinationIdentifier { get; set; }
        /// <summary>
        /// Object values for comparing
        /// </summary>
        public ObjectValuesNode? DestinationObjectValues { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new(base.ToString());
            builder.AppendLine();
            
            if (DestinationType != null)
            {
                builder.AppendLine($"Destination type: {DestinationType}");
            }
            if (DestinationIdentifier != null)
            {
                builder.AppendLine($"Destination identifier: {DestinationIdentifier}");
            }
            if (DestinationObjectValues != null)
            {
                builder.AppendLine($"Object values: {DestinationObjectValues}");
            }

            return builder.ToString().TrimEnd();
        }
    }
}
