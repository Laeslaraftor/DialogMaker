using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents <c>as</c> expression
    /// </summary>
    /// <param name="token">Token that represents <c>as</c> keyword</param>
    public class AsExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression that should be converted
        /// </summary>
        public ExpressionNode? Expression { get; set; }
        /// <summary>
        /// Type for converting
        /// </summary>
        public TypeInfoNode? ConvertType { get; set; }

        #region Static

        /// <summary>
        /// Parse as expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed as expression</returns>
        public static AsExpressionNode Parse(AstParserStream stream)
        {
            var token = stream.Eat(DSharpTokenType.As);
            var type = TypeInfoNode.Parse(stream, true, true);

            return new(token)
            {
                ConvertType = type
            };
        }

        #endregion
    }
}
