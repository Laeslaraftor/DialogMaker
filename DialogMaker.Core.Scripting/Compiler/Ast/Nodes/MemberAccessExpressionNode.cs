using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Member access expression
    /// </summary>
    /// <param name="token">Token that represents access operation</param>
    public class MemberAccessExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression of target
        /// </summary>
        public ExpressionNode? Target { get; set; }
        /// <summary>
        /// Accessed member
        /// </summary>
        public ExpressionNode? Member { get; set; }

        #region Управление

        /// <summary>
        /// Get full name of accessed member
        /// </summary>
        /// <param name="simplifyGenerics">Is generic simplifying needed</param>
        /// <returns>Full bane of accessed member</returns>
        /// <exception cref="InvalidOperationException">Member and target can not be null</exception>
        /// <exception cref="InvalidOperationException">Member must contains identifier</exception>
        /// <exception cref="InvalidOperationException">Target must contains identifier</exception>
        public string GetName(bool simplifyGenerics = false, bool withoutGenerics = false)
        {
            if (Member == null || Target == null)
            {
                throw new InvalidOperationException("Member and target can not be null");
            }

            string targetName;
            string memberName;

            if (Member is IdentifierExpressionNode memberIdentifier)
            {
                if (withoutGenerics)
                {
                    memberName = memberIdentifier.Name;
                }
                else
                {
                    memberName = memberIdentifier.GetName(simplifyGenerics);
                }
            }
            else
            {
                throw new InvalidOperationException("Member must contains identifier");
            }
            if (Target is MemberAccessExpressionNode targetAccess)
            {
                targetName = targetAccess.GetName(simplifyGenerics, withoutGenerics);
            }
            else if (Target is IdentifierExpressionNode targetIdentifier)
            {
                if (withoutGenerics)
                {
                    targetName = targetIdentifier.Name;
                }
                else
                {
                    targetName = targetIdentifier.GetName(simplifyGenerics);
                }
            }
            else
            {
                throw new InvalidOperationException("Target must contains identifier");
            }

            return $"{targetName}.{memberName}";
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Target == null || Member == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Target: {Target}");
            builder.Append($"Member: {Member}");

            return builder.ToString();
        }

        #endregion
    }
}
