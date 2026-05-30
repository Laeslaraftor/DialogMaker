using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Field node
    /// </summary>
    /// <param name="token">Token that represents field name</param>
    public class FieldNode(DialogScriptToken token) : VariableNode(token)
    {
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
        public DialogScriptAccessModifier Access { get; set; } = DialogScriptAccessModifier.Private;
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
        public static FieldNode ParseField(AstParserStream stream, StructNode.MemberInfo memberInfo)
        {
            if (memberInfo.MemberType != DialogScriptTypeMember.Field)
            {
                throw new ArgumentException($"Invalid member info. Requires info for {DialogScriptTypeMember.Field}, provided: {memberInfo.Type}");
            }

            FieldNode field = new(memberInfo.Identifier.Token)
            {
                Attributes = memberInfo.Attributes,
                CanRead = true,
                CanWrite = true,
                IsStatic = memberInfo.IsStatic,
                Access = memberInfo.AccessModifier
            };

            bool ReadGetter()
            {
                if (TryParseAccessor(stream, DialogScriptPropertyAccessor.Getter, out var getterBlock))
                {
                    field.CanRead = true;
                    field.Getter = getterBlock;
                    return true;
                }
                return false;
            }
            bool ReadSetter()
            {
                if (TryParseAccessor(stream, DialogScriptPropertyAccessor.Setter, out var setterBlock))
                {
                    field.CanWrite = true;
                    field.Setter = setterBlock;
                    return true;
                }
                return false;
            }

            if (stream.Check(DialogScriptTokenType.LeftBrace))
            {
                stream.Eat(DialogScriptTokenType.LeftBrace);

                field.CanRead = false;
                field.CanWrite = false;
                field.CustomGetterSetter = true;
                bool getterRead = ReadGetter();

                if (!stream.Check(DialogScriptTokenType.RightBrace))
                {
                    ReadSetter();

                    if (!getterRead && !stream.Check(DialogScriptTokenType.RightBrace))
                    {
                        ReadGetter();
                    }
                }

                stream.Eat(DialogScriptTokenType.RightBrace);
            }
            else if (stream.Check(DialogScriptTokenType.Lambda))
            {
                field.CanWrite = false;
                field.Getter = BlockStatementNode.Parse(stream, DialogScriptTokenType.Semicolon, DialogScriptTokenType.Lambda);
                field.CustomGetterSetter = true;
            }
            else if (stream.Check(DialogScriptTokenType.Assign))
            {
                stream.Eat(DialogScriptTokenType.Assign);
                field.Initializer = ExpressionNode.ParseExpression(stream);
                stream.Eat(DialogScriptTokenType.Semicolon);
            }

            return field;
        }

        private static bool TryParseAccessor(AstParserStream stream, DialogScriptPropertyAccessor accessor, out BlockStatementNode? result)
        {
            var tokenType = (DialogScriptTokenType)accessor;
            result = null;

            if (!stream.Check(tokenType))
            {
                if (!stream.Check((DialogScriptTokenType)accessor.Invert()))
                {
                    stream.ThrowUnexpectedTokenException(tokenType);
                }

                return false;
            }

            stream.Eat(tokenType);

            if (stream.Check(DialogScriptTokenType.Semicolon))
            {
                stream.Eat(DialogScriptTokenType.Semicolon);
                return true;
            }
            else if (stream.Check(DialogScriptTokenType.Lambda))
            {
                result = BlockStatementNode.Parse(stream, DialogScriptTokenType.Semicolon);
                return true;
            }
            else if (stream.Check(DialogScriptTokenType.LeftBrace))
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
