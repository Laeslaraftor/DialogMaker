using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Object (class or struct) declaration node
    /// </summary>
    /// <param name="token">Token that represents object name</param>
    public class ObjectDeclarationNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Identifier (name with generic parameters) of this object type
        /// </summary>
        public IdentifierExpressionNode? Identifier { get; set; }
        /// <summary>
        /// Object type
        /// </summary>
        public DSharpObjectType Type { get; set; }
        /// <summary>
        /// Access modifier of this object
        /// </summary>
        public DSharpAccessModifier Access { get; set; }
        /// <summary>
        /// Is abstract type
        /// </summary>
        public bool IsAbstract { get; set; }
        /// <summary>
        /// Is sealed type
        /// </summary>
        public bool IsSealed { get; set; }
        /// <summary>
        /// Is static type
        /// </summary>
        public bool IsStatic { get; set; }
        /// <summary>
        /// Base types of this object
        /// </summary>
        public List<TypeInfoNode> BaseTypes { get; set; } = [];
        /// <summary>
        /// Field of object
        /// </summary>
        public List<FieldNode> Fields { get; set; } = [];
        /// <summary>
        /// Methods of object
        /// </summary>
        public List<MethodNode> Methods { get; set; } = [];
        /// <summary>
        /// Constructors of struct
        /// </summary>
        public List<ConstructorNode> Constructors { get; set; } = [];
        /// <summary>
        /// Children objects structs
        /// </summary>
        public List<ObjectDeclarationNode> Children { get; set; } = [];
        /// <summary>
        /// Children enums of object
        /// </summary>
        public List<EnumNode> ChildrenEnums { get; set; } = [];
        /// <summary>
        /// Children of object
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
        public static bool TryParseAccessModifier(AstParserStream stream, out DSharpAccessModifier result)
        {
            result = DSharpAccessModifier.Private;
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
        public static bool TryParseAccessModifier(DSharpToken token, out DSharpAccessModifier result)
        {
            result = DSharpAccessModifier.Private;

            foreach (var access in Enum.GetValues(typeof(DSharpAccessModifier)))
            {
                var tokenType = (DSharpTokenType)access;

                if (token.Type == tokenType)
                {
                    result = (DSharpAccessModifier)access;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Try parse member mode from current token and eat it on success
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <param name="result">Token that must be represents member mode</param>
        /// <returns>Member mode successfully parsed</returns>
        public static bool TryParseMemberMode(AstParserStream stream, out DSharpObjectMemberMode result)
        {
            result = DSharpObjectMemberMode.Default;
            var currentToken = stream.Current;

            if (currentToken == null)
            {
                return false;
            }
            if (TryParseMemberMode(currentToken, out result))
            {
                stream.Eat(currentToken.Type);
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try parse member mode from current token
        /// </summary>
        /// <param name="result">Token that must be represents member mode</param>
        /// <returns>Member mode successfully parsed</returns>
        public static bool TryParseMemberMode(DSharpToken token, out DSharpObjectMemberMode result)
        {
            result = DSharpObjectMemberMode.Default;

            if (token.Type == DSharpTokenType.Virtual)
            {
                result = DSharpObjectMemberMode.Virtual;
                return true;
            }
            else if (token.Type == DSharpTokenType.Abstract)
            {
                result = DSharpObjectMemberMode.Abstract;
                return true;
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
            DSharpAccessModifier? accessModifier = null;
            DSharpObjectMemberMode? mode = null;
            var currentToken = stream.Current ?? throw new Exception("Unable to read member");

            if (AttributeNode.TryParse(stream, out var attributeNodes))
            {
                memberInfo.Attributes = attributeNodes;
            }

            bool IsDefinitionEnded(int offset)
            {
                return stream.Check(DSharpTokenType.LeftBrace, offset) || // { - property getter and setter block
                       stream.Check(DSharpTokenType.LeftParen, offset) || // ( - method or constructor
                       stream.Check(DSharpTokenType.Lambda, offset) ||    // => - property lambda
                       stream.Check(DSharpTokenType.Semicolon, offset) || // ; - property without getter and setter
                       stream.Check(DSharpTokenType.Assign, offset);      // = - assign value to property
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
                else if (TryParseMemberMode(currentToken, out var memberMode))
                {
                    if (mode != null)
                    {
                        stream.ThrowPositionException("Multiple virtualizing modifiers");
                    }

                    mode = memberMode;
                    eatToken = true;
                }
                else if (currentToken.Type == DSharpTokenType.Override)
                {
                    if (memberInfo.IsStatic)
                    {
                        stream.ThrowPositionException("Multiple override modifiers");
                    }

                    memberInfo.IsOverride = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DSharpTokenType.ReadOnly)
                {
                    if (memberInfo.IsReadOnly)
                    {
                        stream.ThrowPositionException("Multiple readonly modifiers");
                    }

                    memberInfo.IsReadOnly = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DSharpTokenType.Sealed)
                {
                    if (memberInfo.IsStatic)
                    {
                        stream.ThrowPositionException("Multiple sealed modifiers");
                    }

                    memberInfo.IsSealed = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DSharpTokenType.Static)
                {
                    if (memberInfo.IsStatic)
                    {
                        stream.ThrowPositionException("Multiple static modifiers");
                    }

                    memberInfo.IsStatic = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DSharpTokenType.Extern)
                {
                    if (memberInfo.IsExtern)
                    {
                        stream.ThrowPositionException("Multiple extern modifiers");
                    }

                    memberInfo.IsExtern = true;
                    eatToken = true;
                }
                else if (currentToken.Type == DSharpTokenType.Func)
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
                    if (stream.Check(DSharpTokenType.Identifier) && IsDefinitionEnded(1))
                    {
                        memberInfo.Identifier = IdentifierExpressionNode.Parse(stream);
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

            memberInfo.AccessModifier = accessModifier ?? DSharpAccessModifier.Private;
            memberInfo.Mode = mode ?? DSharpObjectMemberMode.Default;

            if (stream.Check(DSharpTokenType.LeftParen))
            {
                if (memberInfo.Type == null)
                {
                    memberInfo.MemberType = DSharpTypeMember.Constructor;
                    return true;
                }

                memberInfo.MemberType = DSharpTypeMember.Method;
                return true;
            }
            if (memberInfo.Type == null)
            {
                stream.Position = startPosition;
                return false;
            } 

            memberInfo.MemberType = DSharpTypeMember.Field;
            return true;
        }
        /// <summary>
        /// Check current token is access modifier
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Return true when token is access modifier</returns>
        public static bool IsAccessModifier(AstParserStream stream)
        {
            return stream.CheckAll<DSharpAccessModifier>();
        }
        /// <summary>
        /// Check current token is member mode (virtual or abstract)
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <param name="offset">Token offset</param>
        /// <returns>Return true when token is virtual or abstract</returns>
        public static bool IsMemberMode(AstParserStream stream, int offset = 0)
        {
            return stream.Check(DSharpTokenType.Virtual, offset) ||
                   stream.Check(DSharpTokenType.Abstract, offset);
        }
        /// <summary>
        /// Check current and next tokens is object declaration
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser</param>
        /// <returns>Return true when now object definition</returns>
        public static bool IsObjectDeclaration(AstParserStream stream)
        {
            bool IsDeclarationKeyword(int offset)
            {
                return stream.Check(DSharpTokenType.Struct, offset) ||
                       stream.Check(DSharpTokenType.Class, offset) ||
                       stream.Check(DSharpTokenType.Enum, offset) ||
                       stream.Check(DSharpTokenType.Interface, offset);
            }
            bool IsStatic(int offset)
            {
                return stream.Check(DSharpTokenType.Static, offset);
            }

            if (IsDeclarationKeyword(0))
            {
                return true;
            }

            bool isAccessModifier = IsAccessModifier(stream);

            return isAccessModifier && IsDeclarationKeyword(1) ||
                   IsStatic(0) && IsDeclarationKeyword(1) ||
                   IsMemberMode(stream) && IsDeclarationKeyword(1) ||
                   isAccessModifier && IsStatic(1) && IsDeclarationKeyword(2) ||
                   isAccessModifier && IsMemberMode(stream, 1) && IsDeclarationKeyword(2);
        }
        /// <summary>
        /// Parse object node starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed object node</returns>
        /// <exception cref="Exception">Invalid object member</exception>
        public static ObjectDeclarationNode Parse(AstParserStream stream)
        {
            TryParseAccessModifier(stream, out var access);
            bool memberModeParsed = TryParseMemberMode(stream, out var memberMode);
            bool isSealed = false;
            bool isStatic = false;

            if (memberModeParsed && memberMode == DSharpObjectMemberMode.Virtual)
            {
                stream.Position--;
                stream.ThrowPositionException("Object can not be virtual");
            }
            else if (!memberModeParsed && stream.Check(DSharpTokenType.Sealed))
            {
                stream.Eat(DSharpTokenType.Sealed);
                isSealed = true;
            }
            if (stream.Check(DSharpTokenType.Static))
            {
                stream.Eat(DSharpTokenType.Static);
                isStatic = true;
            }

            DSharpObjectType objectType = DSharpObjectType.Class;

            if (stream.Check(DSharpTokenType.Struct))
            {
                stream.Eat(DSharpTokenType.Struct);
                objectType = DSharpObjectType.Struct;
            }
            else if (stream.Check(DSharpTokenType.Interface))
            {
                stream.Eat(DSharpTokenType.Interface);
                objectType = DSharpObjectType.Interface;
            }
            else if (stream.Check(DSharpTokenType.Enum))
            {
                stream.Eat(DSharpTokenType.Enum);
                objectType = DSharpObjectType.Enum;
            }
            else
            {
                stream.Eat(DSharpTokenType.Class);
            }

            var identifier = IdentifierExpressionNode.Parse(stream);
            ObjectDeclarationNode node = new(identifier.Token)
            {
                Identifier = identifier,
                Type = objectType,
                IsAbstract = memberMode == DSharpObjectMemberMode.Abstract,
                IsSealed = isSealed,
                Access = access,
                IsStatic = isStatic
            };

            if (stream.Check(DSharpTokenType.Colon))
            {
                stream.Eat(DSharpTokenType.Colon);

                while (!stream.Check(DSharpTokenType.LeftBrace))
                {
                    var type = TypeInfoNode.Parse(stream, false, false);
                    node.BaseTypes.Add(type);

                    if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DSharpTokenType.LeftBrace))
                    {
                        stream.ThrowPositionException("Required base type");
                    }
                }
            }

            stream.Eat(DSharpTokenType.LeftBrace);

            while (!stream.Check(DSharpTokenType.RightBrace))
            {
                if (stream.Check(DSharpTokenType.Comment) ||
                    stream.Check(DSharpTokenType.MultilineComment))
                {
                    stream.Eat(stream.Current!.Type);
                    continue;
                }
                if (IsObjectDeclaration(stream))
                {
                    var child = Parse(stream);
                    node.Children.Add(child);
                    continue;
                }
                if (!TryStartParseMember(stream, out var memberInfo))
                {
                    stream.ThrowPositionException($"Invalid object member: {stream.Peek()}");
                }

                var member = ParseMember(stream, memberInfo);

                if (member is ConstructorNode constructor)
                {
                    node.Constructors.Add(constructor);
                }
                else if (member is MethodNode method)
                {
                    node.Methods.Add(method);
                }
                else if (member is FieldNode field)
                {
                    node.Fields.Add(field);
                }
                else
                {
                    throw new Exception($"Invalid member type: {memberInfo.MemberType}");
                }
            }

            stream.Eat(DSharpTokenType.RightBrace);

            return node;
        }
        /// <summary>
        /// Parse object member with provided <see cref="MemberInfo"/>
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="memberInfo">Information about member that must be parsed</param>
        /// <param name="attributes">Attributes of member</param>
        /// <returns>Parsed member</returns>
        /// <exception cref="Exception"></exception>
        public static AstNode ParseMember(AstParserStream stream, MemberInfo memberInfo, List<AttributeNode>? attributes = null)
        {
            if (memberInfo.MemberType == DSharpTypeMember.Constructor)
            {
                var constructor = ConstructorNode.Parse(stream, memberInfo);

                if (attributes != null)
                {
                    constructor.Attributes = attributes;
                }

                return constructor;
            }
            else if (memberInfo.MemberType == DSharpTypeMember.Field)
            {
                var field = FieldNode.ParseField(stream, memberInfo);

                if (attributes != null)
                {
                    field.Attributes = attributes;
                }

                return field;
            }
            else if (memberInfo.MemberType == DSharpTypeMember.Method)
            {
                var method = MethodNode.Parse(stream, memberInfo);

                if (attributes != null)
                {
                    method.Attributes = attributes;
                }

                return method;
            }
            else
            {
                throw new Exception($"Invalid member type: {memberInfo.MemberType}");
            }
        }

        /// <summary>
        /// Information about struct member
        /// </summary>
        public struct MemberInfo
        {
            /// <summary>
            /// Type of member
            /// </summary>
            public DSharpTypeMember MemberType { get; set; }
            /// <summary>
            /// Access modifier of this member
            /// </summary>
            public DSharpAccessModifier AccessModifier { get; set; }
            /// <summary>
            /// Member mode
            /// </summary>
            public DSharpObjectMemberMode Mode { get; set; }
            /// <summary>
            /// Is member override
            /// </summary>
            public bool IsOverride { get; set; }
            /// <summary>
            /// Is sealed member
            /// </summary>
            public bool IsSealed { get; set; }
            /// <summary>
            /// Extern flag of member
            /// </summary>
            public bool IsExtern { get; set; }
            /// <summary>
            /// Static flag of member
            /// </summary>
            public bool IsStatic { get; set; }
            public bool IsReadOnly { get; set; }
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
