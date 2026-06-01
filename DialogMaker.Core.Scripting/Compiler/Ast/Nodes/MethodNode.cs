using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Method node
    /// </summary>
    /// <param name="token">Token that represents method name</param>
    public class MethodNode(DSharpToken token) : InvokableNode(token)
    {
        /// <summary>
        /// Method returning type
        /// </summary>
        public TypeInfoNode? ReturnType { get; set; }
        /// <summary>
        /// Extern flag
        /// </summary>
        public bool IsExtern { get; set; }
        /// <summary>
        /// Static flag
        /// </summary>
        public bool IsStatic { get; set; }
        /// <summary>
        /// Override flag
        /// </summary>
        public bool IsOverride { get; set; }
        /// <summary>
        /// Sealed flag
        /// </summary>
        public bool IsSealed { get; set; }
        /// <summary>
        /// Member mode
        /// </summary>
        public DSharpObjectMemberMode Mode { get; set; }
        /// <summary>
        /// Access modifier of this method
        /// </summary>
        public DSharpAccessModifier Access { get; set; } = DSharpAccessModifier.Private;

        #region Статика

        /// <summary>
        /// Parse method starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Information about method that must be parsed</param>
        /// <returns>Parsed method</returns>
        /// <exception cref="ArgumentException">Invalid member info</exception>
        public static MethodNode Parse(AstParserStream stream, ObjectDeclarationNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DSharpTypeMember.Method)
            {
                throw new ArgumentException($"Invalid member info. Requires info for {DSharpTypeMember.Method}, provided: {memberInfo.Type}");
            }

            MethodNode method = new(memberInfo.Identifier.Token)
            {
                Identifier = memberInfo.Identifier,
                Attributes = memberInfo.Attributes,
                Access = memberInfo.AccessModifier,
                ReturnType = memberInfo.Type,
                IsExtern = memberInfo.IsExtern,
                IsStatic = memberInfo.IsStatic,
                IsOverride = memberInfo.IsOverride,
                IsSealed = memberInfo.IsSealed,
                Mode = memberInfo.Mode
            };

            ParseParameters(stream, method.Parameters);

            if (memberInfo.IsExtern)
            {
                stream.Eat(DSharpTokenType.Semicolon);
                return method;
            }
            if (stream.Check(DSharpTokenType.Lambda))
            {
                method.Body = BlockStatementNode.Parse(stream, DSharpTokenType.Semicolon);
            }
            else if (stream.Check(DSharpTokenType.LeftBrace))
            {
                method.Body = BlockStatementNode.Parse(stream);
            }
            else
            {
                stream.ThrowPositionException("Required method body");
            }

            return method;
        }

        #endregion
    }
}
