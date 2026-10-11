using DialogMaker.Core.Scripting.Compiler.Ast.Nodes;
using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Analyzers
{
    /// <summary>
    /// Ast expression analyzer
    /// </summary>
    public static class DSharpExpressionAnalyzer
    {
        /// <summary>
        /// Is expression always returns some value
        /// </summary>
        /// <param name="expression">Expression for checking</param>
        /// <returns>Is always returns value</returns>
        public static bool IsAlwaysReturnsValue(ExpressionNode expression)
        {
            if (expression is MemberAccessExpressionNode memberAccess)
            {
                while (memberAccess.Member is MemberAccessExpressionNode accessedMember)
                {
                    memberAccess = accessedMember;
                }

                var member = memberAccess.Member;

                if (member == null)
                {
                    return false;
                }

                expression = member;
            }
            else if (expression is SwitchExpressionNode switchExpression)
            {
                return switchExpression.Cases.Any(c => c.IsDefault);
            }

            return expression is not CallExpressionNode &&
                   expression is not IncrementExpressionNode &&
                   expression is not DecrementExpressionNode &&
                   expression is not AssignmentExpressionNode;
        }
        /// <summary>
        /// Is expression may return value
        /// </summary>
        /// <param name="expression">Expression for checking</param>
        /// <returns>Is expression may return value</returns>
        public static bool IsMayReturnValue(ExpressionNode expression)
        {
            return expression is CallExpressionNode || 
                   expression is MemberAccessExpressionNode ||
                   expression is SwitchExpressionNode ||
                   IsAlwaysReturnsValue(expression);
        }
    }
}
