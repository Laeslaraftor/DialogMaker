using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents indexer of object: this[int] { get; set; }
    /// </summary>
    /// <param name="token">Token that represents this keyword</param>
    public class IndexerNode(DSharpToken token) : FieldNode(token)
    {
        /// <summary>
        /// Parameters of indexer
        /// </summary>
        public List<VariableNode> Parameters { get; set; } = [];

        #region Статика

        /// <summary>
        /// Parse indexer starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Info about indexer that must be parsed</param>
        /// <returns>Parsed indexer</returns>
        /// <exception cref="ArgumentException">Invalid member info</exception>
        public static IndexerNode ParseIndexer(AstParserStream stream, ObjectDeclarationNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DSharpTypeMember.Indexer)
            {
                throw new ArgumentException($"Invalid member info. Requires info for {DSharpTypeMember.Indexer}, provided: {memberInfo.Type}");
            }

            IndexerNode indexer = new(memberInfo.Identifier.Token)
            {
                Identifier = memberInfo.Identifier,
                Attributes = memberInfo.Attributes,
                CanRead = true,
                CanWrite = true,
                IsReadOnly = memberInfo.IsReadOnly,
                IsStatic = memberInfo.IsStatic,
                Access = memberInfo.AccessModifier,
                Mode = memberInfo.Mode,
                IsOverride = memberInfo.IsOverride,
                IsSealed = memberInfo.IsSealed,
                Type = memberInfo.Type
            };

            InvokableNode.ParseParameters(stream, indexer.Parameters, DSharpTokenType.LeftBracket, DSharpTokenType.RightBracket);

            if (!ParseGetterAndSetter(stream, indexer))
            {
                stream.ThrowPositionException("Unable to read getter and setter of indexer");
            }

            return indexer;
        }

        #endregion
    }
}
