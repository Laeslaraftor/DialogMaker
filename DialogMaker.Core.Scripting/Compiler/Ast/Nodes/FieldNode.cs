using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Field node
    /// </summary>
    /// <param name="token">Token that represents field name</param>
    public class FieldNode(DSharpToken token) : VariableNode(token)
    {
        /// <summary>
        /// Identifier of this field
        /// </summary>
        public IdentifierExpressionNode? Identifier { get; set; }
        /// <summary>
        /// Can read flag
        /// </summary>
        public bool CanRead { get; set; }
        /// <summary>
        /// Can write flag
        /// </summary>
        public bool CanWrite { get; set; }
        /// <summary>
        /// Custom getter block
        /// </summary>
        public BlockStatementNode? Getter { get; set; }
        /// <summary>
        /// Custom setter block
        /// </summary>
        public BlockStatementNode? Setter { get; set; }
        /// <summary>
        /// Custom getter or/and setter existence flag
        /// </summary>
        public bool CustomGetterSetter { get; set; }
        /// <summary>
        /// Access modifier for this field
        /// </summary>
        public DSharpAccessModifier Access { get; set; } = DSharpAccessModifier.Private;
        /// <summary>
        /// Is static flag
        /// </summary>
        public bool IsStatic { get; set; }

        #region Статика

        /// <summary>
        /// Parse field starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Info about field that must be parsed</param>
        /// <returns>Parsed field</returns>
        /// <exception cref="ArgumentException">Invalid member info</exception>
        public static FieldNode ParseField(AstParserStream stream, ObjectDeclarationNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DSharpTypeMember.Field)
            {
                throw new ArgumentException($"Invalid member info. Requires info for {DSharpTypeMember.Field}, provided: {memberInfo.Type}");
            }

            FieldNode field = new(memberInfo.Identifier.Token)
            {
                Identifier = memberInfo.Identifier,
                Attributes = memberInfo.Attributes,
                CanRead = true,
                CanWrite = true,
                IsStatic = memberInfo.IsStatic,
                Access = memberInfo.AccessModifier
            };

            bool ReadGetter()
            {
                if (TryParseAccessor(stream, DSharpPropertyAccessor.Getter, out var getterBlock))
                {
                    field.CanRead = true;
                    field.Getter = getterBlock;
                    return true;
                }
                return false;
            }
            bool ReadSetter()
            {
                if (TryParseAccessor(stream, DSharpPropertyAccessor.Setter, out var setterBlock))
                {
                    field.CanWrite = true;
                    field.Setter = setterBlock;
                    return true;
                }
                return false;
            }

            if (stream.Check(DSharpTokenType.LeftBrace))
            {
                stream.Eat(DSharpTokenType.LeftBrace);

                field.CanRead = false;
                field.CanWrite = false;
                field.CustomGetterSetter = true;
                bool getterRead = ReadGetter();

                if (!stream.Check(DSharpTokenType.RightBrace))
                {
                    ReadSetter();

                    if (!getterRead && !stream.Check(DSharpTokenType.RightBrace))
                    {
                        ReadGetter();
                    }
                }

                stream.Eat(DSharpTokenType.RightBrace);
            }
            else if (stream.Check(DSharpTokenType.Lambda))
            {
                field.CanWrite = false;
                field.Getter = BlockStatementNode.Parse(stream, DSharpTokenType.Semicolon, DSharpTokenType.Lambda);
                field.CustomGetterSetter = true;
            }
            else if (stream.Check(DSharpTokenType.Assign))
            {
                stream.Eat(DSharpTokenType.Assign);
                field.Initializer = ExpressionNode.ParseExpression(stream);
                stream.Eat(DSharpTokenType.Semicolon);
            }

            return field;
        }

        private static bool TryParseAccessor(AstParserStream stream, DSharpPropertyAccessor accessor, out BlockStatementNode? result)
        {
            var tokenType = (DSharpTokenType)accessor;
            result = null;

            if (!stream.Check(tokenType))
            {
                if (!stream.Check((DSharpTokenType)accessor.Invert()))
                {
                    stream.ThrowUnexpectedTokenException(tokenType);
                }

                return false;
            }

            stream.Eat(tokenType);

            if (stream.Check(DSharpTokenType.Semicolon))
            {
                stream.Eat(DSharpTokenType.Semicolon);
                return true;
            }
            else if (stream.Check(DSharpTokenType.Lambda))
            {
                result = BlockStatementNode.Parse(stream, DSharpTokenType.Semicolon);
                return true;
            }
            else if (stream.Check(DSharpTokenType.LeftBrace))
            {
                result = BlockStatementNode.Parse(stream);
                return true;
            }
            else
            {
                stream.ThrowUnexpectedTokenException();
            }

            return false;
        }

        #endregion
    }
}
