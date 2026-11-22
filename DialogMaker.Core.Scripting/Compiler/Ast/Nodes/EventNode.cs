using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents event
    /// </summary>
    /// <param name="token">Token that represents event identifier</param>
    public partial class EventNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Event handler delegate type
        /// </summary>
        public TypeInfoNode? Type { get; set; }
        /// <summary>
        /// Is static event
        /// </summary>
        public bool IsStatic { get; set; }
        /// <summary>
        /// Is unsafe event
        /// </summary>
        public bool IsUnsafe { get; set; }
        /// <summary>
        /// Event access modifier
        /// </summary>
        public DSharpAccessModifier Access { get; set; }
        /// <summary>
        /// Method for add event handler
        /// </summary>
        public BlockStatementNode? Add { get; set; }
        /// <summary>
        /// Method for removing event handler
        /// </summary>
        public BlockStatementNode? Remove { get; set; }
        /// <summary>
        /// Event attributes
        /// </summary>
        public List<AttributeNode>? Attributes { get; set; }

        #region Static

        /// <summary>
        /// Parse event node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Information about event member</param>
        /// <returns>Parsed event node</returns>
        public static EventNode Parse(AstParserStream stream, ObjectDeclarationNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DSharpTypeMember.Event)
            {
                stream.ThrowPositionException($"Required event member info, got {memberInfo.MemberType}");
            }

            stream.Eat(DSharpTokenType.Event);

            var type = TypeInfoNode.Parse(stream, false, false);
            var identifier = stream.Eat(DSharpTokenType.Identifier);
            EventNode node = new(identifier)
            {
                Access = memberInfo.AccessModifier,
                IsStatic = memberInfo.IsStatic,
                IsUnsafe = memberInfo.IsStatic,
                Attributes = memberInfo.Attributes,
                Type = type
            };

            if (stream.Check(DSharpTokenType.Semicolon))
            {
                stream.Eat(DSharpTokenType.Semicolon);
            }
            else
            {
                stream.Eat(DSharpTokenType.LeftBrace);
                ParseAddAndRemove(stream, node);
                stream.Eat(DSharpTokenType.RightBrace);
            }

            return node;
        }

        private static void ParseAddAndRemove(AstParserStream stream, EventNode node)
        {
            BlockStatementNode ParseBlock(DSharpTokenType token)
            {
                stream.Eat(token);

                if (stream.Check(DSharpTokenType.Lambda))
                {
                    return BlockStatementNode.Parse(stream, DSharpStatementType.Code, DSharpTokenType.Semicolon, DSharpTokenType.Lambda);
                }

                return BlockStatementNode.Parse(stream, DSharpStatementType.Code);
            }
            bool Parse(DSharpTokenType token)
            {
                if (!stream.Check(token))
                {
                    return false;
                }

                node.Add = ParseBlock(token);

                return true;
            }

            var addParsed = Parse(DSharpTokenType.Add);
            Parse(DSharpTokenType.Remove);

            if (!addParsed)
            {
                Parse(DSharpTokenType.Add);
            }
        }

        #endregion
    }
}
