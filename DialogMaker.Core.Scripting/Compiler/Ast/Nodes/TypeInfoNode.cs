using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents type identifier. 
    /// For example: object, string, number, bool, SomeType, Some.Type, object?, object[], SomeType?[]?
    /// </summary>
    /// <param name="token">Token at start of type</param>
    public class TypeInfoNode(DialogScriptToken token) : NamedNode(token)
    {
        /// <summary>
        /// Flag which indicate when type can be nullable
        /// </summary>
        public bool IsNullable { get; set; }
        /// <summary>
        /// Amount of array dimensions. Type is not array When value equals 0
        /// </summary>
        public int ArrayDimensions { get; set; }
        /// <summary>
        /// Nullable flags for array dimensions. Size of this list must be equals to <see cref="ArrayDimensions"/>
        /// </summary>
        public List<bool> ArrayNullability { get; set; } = [];

        #region Статика

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
            var current = stream.Current ?? throw new Exception("Unable to read type identifier");
            TypeInfoNode result;

            if (current.Type == DialogScriptTokenType.String ||
                current.Type == DialogScriptTokenType.Number ||
                current.Type == DialogScriptTokenType.Bool ||
                current.Type == DialogScriptTokenType.Object ||
                current.Type == DialogScriptTokenType.Var)
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
                else if (accessExpression is IdentifierExpressionNode)
                {
                    result = new(accessExpression.Token);
                }
                else
                {
                    throw new Exception($"Invalid expression provided: {accessExpression}, required {nameof(MemberAccessExpressionNode)} or {nameof(IdentifierExpressionNode)}");
                }
            }

            bool CheckNullable()
            {
                if (stream.Check(DialogScriptTokenType.Question))
                {
                    stream.Eat(DialogScriptTokenType.Question);
                    return true;
                }

                return false;
            }

            result.IsNullable = CheckNullable();

            if (skipArrayCheck)
            {
                return result;
            }

            while (stream.Check(DialogScriptTokenType.LeftBracket))
            {
                if (!canBeArray)
                {
                    stream.ThrowPositionException("Array not allowed in this context");
                }

                stream.Eat(DialogScriptTokenType.LeftBracket);
                stream.Eat(DialogScriptTokenType.RightBracket);
                result.ArrayNullability.Add(CheckNullable());
                result.ArrayDimensions++;
            }

            return result;
        }

        #endregion
    }
}
