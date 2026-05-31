using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Base class of new expression
    /// </summary>
    /// <param name="token">Token that represents new keyword</param>
    public abstract class NewExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Type that new instancing
        /// </summary>
        public TypeInfoNode? Type { get; set; }

        #region Статика

        /// <summary>
        /// Parse new expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed new expression</returns>
        public static NewExpressionNode Parse(AstParserStream stream)
        {
            var newKeyword = stream.Eat(DSharpTokenType.New);
            TypeInfoNode? type = null;
            NewExpressionNode expression;

            if (TypeInfoNode.CanParseIdentifier(stream))
            {
                type = TypeInfoNode.ParseOnlyIdentifier(stream, true);
            }
            if (stream.Check(DSharpTokenType.LeftBracket))
            {
                expression = NewArrayExpressionNode.Parse(stream, newKeyword);
            }
            else
            {
                expression = NewInstanceExpressionNode.Parse(stream, newKeyword, type);
            }

            expression.Type = type;

            return expression;
        }

        #endregion
    }
}
