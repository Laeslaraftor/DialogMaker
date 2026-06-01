using DialogMaker.Core.Scripting.Compiler.Lexer;

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

        public string GetName(bool simplifyGenerics = false)
        {
            if (Member == null || Target == null)
            {
                throw new InvalidOperationException($"Member and target can not be null");
            }

            string targetName;
            string memberName;

            if (Member is IdentifierExpressionNode memberIdentifier)
            {
                memberName = memberIdentifier.GetName(simplifyGenerics);
            }
            else
            {
                throw new InvalidOperationException($"Member must contains identifier");
            }
            if (Target is MemberAccessExpressionNode targetAccess)
            {
                targetName = GetName(simplifyGenerics);
            }
            else if (Target is IdentifierExpressionNode targetIdentifier)
            {
                targetName = targetIdentifier.GetName(simplifyGenerics);
            }
            else
            {
                throw new InvalidOperationException($"Target must contains identifier");
            }

            return $"{targetName}.{memberName}";
        }

        #endregion
    }
}
