using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Identifier expression
    /// </summary>
    /// <param name="token">Token that represents identifier (name)</param>
    public class IdentifierExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        public List<TypeInfoNode> GenericParameters { get; set; } = [];

        #region Управление

        public string GetName(bool simplifyGenerics)
        {
            return Name + GenericParameters.GetGenericsName(simplifyGenerics);
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse identifier expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="parseGenericParameters">Flag which indicates that generic parameters must be parsed</param>
        /// <returns>Parsed identifier expression</returns>
        public static IdentifierExpressionNode Parse(AstParserStream stream, bool parseGenericParameters = true)
        {
            DSharpToken token;
            var currentType = stream.Current?.Type ?? throw new ArgumentException("Unable to parse identifier expression");

            if (stream.Check(DSharpTokenType.Identifier) ||
                TypeInfoNode.IsStandardTypeIdentifier(currentType))
            {
                token = stream.Eat(currentType);
            }
            else
            {
                stream.ThrowPositionException($"Required identifier or standard type, got: {currentType}");
                return null;
            }

            IdentifierExpressionNode expression = new(token);

            if (parseGenericParameters)
            {
                TypeInfoNode.ParseGenericParameters(stream, expression.GenericParameters, true);
            }

            return expression;
        }

        #endregion
    }
}
