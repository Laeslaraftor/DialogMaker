using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Constructor node
    /// </summary>
    /// <param name="token">Token that represents name of constructor</param>
    public class ConstructorNode(DialogScriptToken token) : InvokableNode(token)
    {
        /// <summary>
        /// Access modifier of this constructor
        /// </summary>
        public DialogScriptAccessModifier Access { get; set; } = DialogScriptAccessModifier.Private;

        #region Статика

        /// <summary>
        /// Parse constructor starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Information about constructor that must be parsed</param>
        /// <returns>Parsed constructor</returns>
        /// <exception cref="ArgumentException">Invalid member info</exception>
        public static ConstructorNode Parse(AstParserStream stream, StructNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DialogScriptTypeMember.Constructor)
            {
                throw new ArgumentException($"Invalid member info. Requires info for {DialogScriptTypeMember.Constructor}, provided: {memberInfo.Type}");
            }

            ConstructorNode constructor = new(memberInfo.Identifier.Token)
            {
                Attributes = memberInfo.Attributes,
                Access = memberInfo.AccessModifier
            };

            ParseParameters(stream, constructor.Parameters);

            if (stream.Check(DialogScriptTokenType.LeftBrace))
            {
                constructor.Body = BlockStatementNode.Parse(stream);
            }
            else
            {
                stream.ThrowPositionException("Required method body");
            }

            return constructor;
        }

        #endregion
    }
}
