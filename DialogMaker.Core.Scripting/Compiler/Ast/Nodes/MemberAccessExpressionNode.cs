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
        /// <summary>
        /// Member access mode
        /// </summary>
        public DSharpMemberAccessMode AccessMode { get; set; }

        #region Controls

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
            else if (Member is MemberAccessExpressionNode memberAccess)
            {
                memberName = memberAccess.GetName(simplifyGenerics, withoutGenerics);
            }
            else
            {
                throw new InvalidOperationException($"Member must contains identifier: {this}");
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

        #region Static

        /// <summary>
        /// Check is current token represents access to member
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="offset">Check offset</param>
        /// <returns>Is current token represents access to member</returns>
        public static bool IsAccess(AstParserStream stream, int offset = 0)
        {
            return stream.Check(DSharpTokenType.Dot, offset) ||
                   stream.Check(DSharpTokenType.Greater, offset + 1) &&
                   stream.Check(DSharpTokenType.Minus, offset);
        }
        public static bool TryParseAccessMode(AstParserStream stream, out DSharpMemberAccessMode mode)
        {
            return TryParseAccessMode(stream, true, out _, out mode);
        }
        /// <summary>
        /// Try to parse member access mode start with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="token">First eaten token</param>
        /// <param name="mode">Parse member access mode</param>
        /// <returns>Is member access mode successfully parsed</returns>
        public static bool TryParseAccessMode(AstParserStream stream, out DSharpToken? token, out DSharpMemberAccessMode mode)
        {
            return TryParseAccessMode(stream, true, out token, out mode);
        }
        /// <summary>
        /// Try to parse member access mode start with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="eat">Eat access tokens</param>
        /// <param name="token">First eaten token</param>
        /// <param name="mode">Parse member access mode</param>
        /// <returns>Is member access mode successfully parsed</returns>
        public static bool TryParseAccessMode(AstParserStream stream, bool eat, out DSharpToken? token, out DSharpMemberAccessMode mode)
        {
            mode = DSharpMemberAccessMode.Reference;
            token = null;

            if (stream.Check(DSharpTokenType.Dot))
            {
                if (eat)
                {
                    token = stream.Eat(DSharpTokenType.Dot);
                }

                return true;
            }
            else if (stream.Check(DSharpTokenType.Minus) &&
                     stream.Check(DSharpTokenType.Greater, 1))
            {
                if (eat)
                {
                    token = stream.Eat(DSharpTokenType.Minus);
                    stream.Eat(DSharpTokenType.Greater);
                }
                
                mode = DSharpMemberAccessMode.Pointer;
                return true;
            }

            return false;
        }

        #endregion
    }
}
