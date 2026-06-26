using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents type identifier. 
    /// For example: object, string, number, bool, SomeType, Some.Type, object?, object[], SomeType?[]?
    /// </summary>
    /// <param name="token">Token at start of type</param>
    public class TypeInfoNode(DSharpToken token) : AstNode(token)
    {
        /// <summary>
        /// Flag which indicate when type can be nullable
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// Amount of array dimensions. Type is not array when value equals 0
        /// </summary>
        public int ArrayDimensions { get; set; }
        /// <summary>
        /// Nullable flags for array dimensions. Size of this list must be equals to <see cref="ArrayDimensions"/>
        /// </summary>
        public List<bool> ArrayNullability { get; set; } = [];
        /// <summary>
        /// Generic parameters of this type
        /// </summary>
        public List<TypeInfoNode> GenericParameters { get; set; } = [];

        #region Управление

        public virtual string GetSimpleFullName() => Name;
        public string GetFullName(bool simplifyGenerics, bool nullable)
        {
            string result = GetTypeName(simplifyGenerics);

            if (nullable && IsNullable)
            {
                result += "?";
            }

            result += GenericParameters.GetGenericsName(simplifyGenerics);

            if (!simplifyGenerics)
            {
                foreach (var arrayNullable in ArrayNullability)
                {
                    result += "[]";

                    if (nullable && arrayNullable)
                    {
                        result += "?";
                    }
                }
            }

            return result;
        }

        protected virtual string GetTypeName(bool simplifyGenerics)
        {
            return Name;
        }

        #endregion

        #region Статика

        /// <summary>
        /// Check token type is standard type identifier
        /// </summary>
        /// <param name="type">Token type</param>
        /// <param name="allowVarToken">Allows to <see cref="DSharpTokenType.Var"/></param>
        /// <returns>Is token standard type identifier</returns>
        public static bool IsStandardTypeIdentifier(DSharpTokenType type, bool allowVarToken = true)
        {
            return type == DSharpTokenType.String ||
                   type == DSharpTokenType.Number ||
                   type == DSharpTokenType.Bool ||
                   type == DSharpTokenType.Char ||
                   type == DSharpTokenType.Object ||
                   type == DSharpTokenType.Var && allowVarToken;
        }
        /// <summary>
        /// Check identifier parse availability
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="offset">Token offset</param>
        /// <returns>Is parse identifier available</returns>
        /// <exception cref="Exception">Unable to read type identifier</exception>
        public static bool CanParseIdentifier(AstParserStream stream, int offset = 0)
        {
            var current = stream.Peek(offset) ?? throw new Exception("Unable to read type identifier");

            return IsStandardTypeIdentifier(current.Type) ||
                   stream.Check(DSharpTokenType.Identifier);
        }
        /// <summary>
        /// Parse type info with only name of type starts with current token at parser stream
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="canBePrimary">Indicates that parsed type may be primary (object, string, number, bool, var)</param>
        /// <returns>Parsed type node</returns>
        /// <exception cref="Exception">Unable to read type identifier</exception>
        public static TypeInfoNode ParseOnlyIdentifier(AstParserStream stream, bool canBePrimary)
        {
            var current = stream.Current ?? throw new Exception("Unable to read type identifier");
            TypeInfoNode result;

            if (IsStandardTypeIdentifier(current.Type))
            {
                if (!canBePrimary)
                {
                    stream.ThrowPositionException("Invalid type identifier");
                }

                var primaryTypeToken = stream.Eat(current.Type);

                result = new(primaryTypeToken);
            }
            else
            {
                var accessExpression = ExpressionNode.ParseIdentifier(stream);

                if (accessExpression is MemberAccessExpressionNode memberAccess)
                {
                    result = new MemberTypeInfoNode(accessExpression.Token)
                    {
                        Member = memberAccess
                    };
                }
                else if (accessExpression is IdentifierExpressionNode identifier)
                {
                    result = new(accessExpression.Token)
                    {
                        GenericParameters = identifier.GenericParameters
                    };
                }
                else
                {
                    throw new Exception($"Invalid expression provided: {accessExpression}, required {nameof(MemberAccessExpressionNode)} or {nameof(IdentifierExpressionNode)}");
                }
            }

            return result;
        }
        /// <summary>
        /// Parse type info starts with current token at parser stream
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="canBePrimary">Indicates that parsed type may be primary (object, string, number, bool, var)</param>
        /// <param name="canBeArray">Indicates that parsed type may be array</param>
        /// <returns>Parsed type node</returns>
        /// <exception cref="Exception">Unable to read type identifier</exception>
        public static TypeInfoNode Parse(AstParserStream stream, bool canBePrimary, bool canBeArray, bool skipArrayCheck = false)
        {
            var result = ParseOnlyIdentifier(stream, canBePrimary);

            bool CheckNullable()
            {
                if (stream.Check(DSharpTokenType.Question))
                {
                    stream.Eat(DSharpTokenType.Question);
                    return true;
                }

                return false;
            }

            result.IsNullable = CheckNullable();
            ParseGenericParameters(stream, result.GenericParameters, true);

            if (skipArrayCheck)
            {
                return result;
            }

            while (stream.Check(DSharpTokenType.LeftBracket))
            {
                if (!canBeArray)
                {
                    stream.ThrowPositionException("Array not allowed in this context");
                }

                stream.Eat(DSharpTokenType.LeftBracket);
                stream.Eat(DSharpTokenType.RightBracket);
                result.ArrayNullability.Add(CheckNullable());
                result.ArrayDimensions++;
            }

            return result;
        }
        public static void ParseGenericParameters(AstParserStream stream, List<TypeInfoNode> buffer, bool checkExistence = false)
        {
            if (checkExistence)
            {
                if (!stream.Check(DSharpTokenType.Less))
                {
                    return;
                }

                int offset = 1;
                int inner = 1;

                bool IsEnd(int offset)
                {
                    return stream.Check(DSharpTokenType.Semicolon, offset) ||
                           stream.Check(DSharpTokenType.Assign, offset) ||
                           stream.Check(DSharpTokenType.LeftParen, offset) ||
                           stream.Check(DSharpTokenType.LeftBrace, offset) ||
                           stream.Check(DSharpTokenType.LeftBracket, offset);
                }

                while (!IsEnd(offset))
                {
                    if (stream.Check(DSharpTokenType.Less, offset))
                    {
                        inner++;
                    }
                    else if (stream.Check(DSharpTokenType.Greater, offset))
                    {
                        inner--;

                        if (0 >= inner)
                        {
                            break;
                        }
                    }
                    else if (stream.Check(DSharpTokenType.Semicolon, offset) ||
                             stream.Check(DSharpTokenType.And, offset))
                    {
                        return;
                    }
                    if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DSharpTokenType.Greater))
                    {
                        return;
                    }

                    offset++;
                }

                if (inner != 0)
                {
                    return;
                }
            }

            stream.Eat(DSharpTokenType.Less);

            while (!stream.Check(DSharpTokenType.Greater))
            {
                var type = Parse(stream, true, true);
                buffer.Add(type);

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream, DSharpTokenType.Greater))
                {
                    stream.ThrowPositionException("Required type identifier");
                }
            }

            stream.Eat(DSharpTokenType.Greater);
        }
        public static List<TypeInfoNode> ParseGenericParameters(AstParserStream stream, bool checkExistence = false)
        {
            List<TypeInfoNode> buffer = [];
            ParseGenericParameters(stream, buffer, checkExistence);

            return buffer;
        }

        #endregion
    }
}
