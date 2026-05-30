using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Struct node
    /// </summary>
    /// <param name="token">Token that represents struct name</param>
    public class StructNode(DialogScriptToken token) : NamedNode(token)
    {
        /// <summary>
        /// Access modifier of this struct
        /// </summary>
        public DialogScriptAccessModifier Access { get; set; }
        /// <summary>
        /// Base types of this struct
        /// </summary>
        public List<NamedNode> BaseTypes { get; set; } = [];
        /// <summary>
        /// Field of structs
        /// </summary>
        public List<FieldNode> Fields { get; set; } = [];
        /// <summary>
        /// Methods of structs
        /// </summary>
        public List<MethodNode> Methods { get; set; } = [];
        /// <summary>
        /// Constructors of struct
        /// </summary>
        public List<ConstructorNode> Constructors { get; set; } = [];
        /// <summary>
        /// Children structs
        /// </summary>
        public List<StructNode> Children { get; set; } = [];
        /// <summary>
        /// Children structs
        /// </summary>
        public List<EnumNode> ChildrenEnums { get; set; } = [];
        /// <summary>
        /// Children structs
        /// </summary>
        public List<AttributeNode>? Attributes { get; set; }

        #region Константы

        /// <summary>
        /// Name of static modifier for methods and fields
        /// </summary>
        public const string StaticModifier = "static";
        /// <summary>
        /// Name of extern modifier for methods
        /// </summary>
        public const string ExternModifier = "extern";

        #endregion

        #region Статика

        /// <summary>
        /// Try parse access modifier from current token and eat it on success
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <param name="result">Parsed access modifier</param>
        /// <returns>Modifier successfully parsed</returns>
        public static bool TryParseAccessModifier(AstParserStream stream, out DialogScriptAccessModifier result)
        {
            result = DialogScriptAccessModifier.Private;
            var currentToken = stream.Current;

            if (currentToken == null)
            {
                return false;
            }
            if (TryParseAccessModifier(currentToken, out result))
            {
                stream.Eat(currentToken.Type);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try parse access modifier from current token
        /// </summary>
        /// <param name="token">Token that represents access modifier</param>
        /// <param name="result">Parsed access modifier</param>
        /// <returns>Modifier successfully parsed</returns>
        public static bool TryParseAccessModifier(DialogScriptToken token, out DialogScriptAccessModifier result)
        {
            result = DialogScriptAccessModifier.Private;

            foreach (var access in Enum.GetValues(typeof(DialogScriptAccessModifier)))
            {
                var tokenType = (DialogScriptTokenType)access;

                if (token.Type == tokenType)
                {
                    result = (DialogScriptAccessModifier)access;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Try to start parse member. This method trying read member modifiers, type and name, then detect what it is.
        /// It restores parser position on fail.
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <param name="memberInfo">Info about parsed member</param>
        /// <returns>Member successfully parsed</returns>
        /// <exception cref="Exception">Unable to read member</exception>
        public static bool TryStartParseMember(AstParserStream stream, out MemberInfo memberInfo)
        {
            memberInfo = new();
            var startPosition = stream.Position;
            DialogScriptAccessModifier? accessModifier = null;
            var currentToken = stream.Current ?? throw new Exception("Unable to read member");

            if (AttributeNode.TryParse(stream, out var attributeNodes))
            {
                memberInfo.Attributes = attributeNodes;
            }

            bool IsDefinitionEnded(int offset)
            {
                return stream.Check(DialogScriptTokenType.LeftBrace, offset) || // { - property getter and setter block
                       stream.Check(DialogScriptTokenType.LeftParen, offset) || // ( - method or constructor
                       stream.Check(DialogScriptTokenType.Lambda, offset) ||    // => - property lambda
                       stream.Check(DialogScriptTokenType.Semicolon, offset) || // ; - property without getter and setter
                       stream.Check(DialogScriptTokenType.Assign, offset);      // = - assign value to property
            }

            do
            {
                bool eatToken = false;

                if (TryParseAccessModifier(currentToken, out var access))
                {
                    if (accessModifier != null)
                    {
                        stream.ThrowPositionException("Multiple access modifiers");
                    }

                    accessModifier = access;
                    eatToken = true;
                }
                else if (currentToken.Type == DialogScriptTokenType.Static)
                {
                    if (memberInfo.IsStatic)
                    {
                        stream.ThrowPositionException("Multiple static modifiers");
                    }

                    memberInfo.IsStatic = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DialogScriptTokenType.Extern)
                {
                    if (memberInfo.IsExtern)
                    {
                        stream.ThrowPositionException("Multiple extern modifiers");
                    }

                    memberInfo.IsExtern = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DialogScriptTokenType.Func)
                {
                    if (memberInfo.Type != null)
                    {
                        stream.ThrowPositionException("Multiple type identifiers");
                    }

                    memberInfo.Type = new(currentToken);
                    eatToken = true;
                }
                else
                {
                    if (stream.Check(DialogScriptTokenType.Identifier) && IsDefinitionEnded(1))
                    {
                        memberInfo.Identifier = new(stream.Eat(DialogScriptTokenType.Identifier));
                        break;
                    }

                    try
                    {
                        memberInfo.Type = TypeInfoNode.Parse(stream, true, true);
                        continue;
                    }
                    catch
                    {
                    }
                }

                if (eatToken)
                {
                    stream.Eat(currentToken.Type);
                    currentToken = stream.Current;
                    continue;
                }

                stream.Position = startPosition;
                return false;
            }
            while (!IsDefinitionEnded(0));

            if (memberInfo.Identifier == null)
            {
                stream.Position = startPosition;
                return false;
            }

            memberInfo.AccessModifier = accessModifier ?? DialogScriptAccessModifier.Private;

            if (stream.Check(DialogScriptTokenType.LeftParen))
            {
                if (memberInfo.Type == null)
                {
                    memberInfo.MemberType = DialogScriptTypeMember.Constructor;
                    return true;
                }

                memberInfo.MemberType = DialogScriptTypeMember.Method;
                return true;
            }

            memberInfo.MemberType = DialogScriptTypeMember.Field;
            return true;
        }
        /// <summary>
        /// Check current token is access modifier
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Return true when token is access modifier</returns>
        public static bool IsAccessModifier(AstParserStream stream)
        {
            return stream.CheckAll<DialogScriptAccessModifier>();
        }
        /// <summary>
        /// Check current and next tokens is struct declaration
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Return true when now struct definition</returns>
        public static bool IsStructDeclaration(AstParserStream stream)
        {
            if (stream.Check(DialogScriptTokenType.Struct))
            {
                return true;
            }

            return IsAccessModifier(stream) && stream.Check(DialogScriptTokenType.Struct, 1);
        }
        /// <summary>
        /// Parse struct node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed struct node</returns>
        /// <exception cref="Exception">Invalid struct member</exception>
        public static StructNode Parse(AstParserStream stream)
        {
            TryParseAccessModifier(stream, out var access);

            stream.Eat(DialogScriptTokenType.Struct);

            var identifier = stream.Eat(DialogScriptTokenType.Identifier);
            StructNode node = new(identifier)
            {
                Access = access
            };

            if (stream.Check(DialogScriptTokenType.Colon)) 
            {
                stream.Eat(DialogScriptTokenType.Colon);

                while (!stream.Check(DialogScriptTokenType.LeftBrace))
                {
                    var type = TypeInfoNode.Parse(stream, false, false);
                    node.BaseTypes.Add(type);

                    if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DialogScriptTokenType.LeftBrace))
                    {
                        stream.ThrowPositionException("Required base type");
                    }
                }
            }

            stream.Eat(DialogScriptTokenType.LeftBrace);

            while (!stream.Check(DialogScriptTokenType.RightBrace))
            {
                if (stream.Check(DialogScriptTokenType.Enum))
                {
                    var enumNode = EnumNode.Parse(stream);
                    node.ChildrenEnums.Add(enumNode);
                }
                if (IsStructDeclaration(stream))
                {
                    var child = Parse(stream);
                    node.Children.Add(child);
                }
                if (!TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException("Invalid struct member");
                }

                if (memberInfo.MemberType == DialogScriptTypeMember.Constructor)
                {
                    var constructor = ConstructorNode.Parse(stream, memberInfo);
                    node.Constructors.Add(constructor);
                }
                else if (memberInfo.MemberType == DialogScriptTypeMember.Field)
                {
                    var field = FieldNode.ParseField(stream, memberInfo);
                    node.Fields.Add(field);
                }
                else if (memberInfo.MemberType == DialogScriptTypeMember.Method)
                {
                    var method = MethodNode.Parse(stream, memberInfo);
                    node.Methods.Add(method);
                }
                else
                {
                    throw new Exception($"Invalid member type: {memberInfo.MemberType}");
                }
            }

            stream.Eat(DialogScriptTokenType.RightBrace);

            return node;
        }

        /// <summary>
        /// Information about struct member
        /// </summary>
        public struct MemberInfo
        {
            /// <summary>
            /// Type of member
            /// </summary>
            public DialogScriptTypeMember MemberType { get; set; }
            /// <summary>
            /// Access modifier of this member
            /// </summary>
            public DialogScriptAccessModifier AccessModifier { get; set; }
            /// <summary>
            /// Extern flag of member
            /// </summary>
            public bool IsExtern { get; set; }
            /// <summary>
            /// Static flag of member
            /// </summary>
            public bool IsStatic { get; set; }
            /// <summary>
            /// Member's return type
            /// </summary>
            public TypeInfoNode? Type { get; set; }
            /// <summary>
            /// Identifier (name) of this member
            /// </summary>
            public IdentifierExpressionNode Identifier { get; set; }
            /// <summary>
            /// List of attributes or this member
            /// </summary>
            public List<AttributeNode>? Attributes { get; set; }
        }

        #endregion
    }
}
