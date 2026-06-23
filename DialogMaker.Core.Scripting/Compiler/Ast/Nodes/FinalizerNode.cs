using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents finalizer/destructor of object
    /// </summary>
    /// <param name="token">Token that represents identifier of finalizer</param>
    public class FinalizerNode(DSharpToken token) : InvokableNode(token)
    {
        #region Статика

        /// <summary>
        /// Parse finalizer node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Information about finalizer</param>
        /// <returns>Parsed finalizer node</returns>
        /// <exception cref="ArgumentException">Member information should contains information for finalizer</exception>
        public static FinalizerNode Parse(AstParserStream stream, ObjectDeclarationNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DSharpTypeMember.Finalizer)
            {
                throw new ArgumentException($"Member information should contains information for finalizer, got: {memberInfo.MemberType}", nameof(memberInfo));
            }

            FinalizerNode finalizer = new(memberInfo.Identifier.Token)
            {
                Identifier = memberInfo.Identifier,
                Attributes = memberInfo.Attributes
            };

            ParseParameters(stream, finalizer.Parameters);
            finalizer.Body = BlockStatementNode.Parse(stream);

            return finalizer;
        }

        #endregion
    }
}
