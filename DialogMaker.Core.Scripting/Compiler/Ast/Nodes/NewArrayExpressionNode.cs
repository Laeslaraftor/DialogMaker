using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// New array instance expression
    /// </summary>
    /// <param name="token"><inheritdoc/></param>
    public class NewArrayExpressionNode(DSharpToken token) : NewExpressionNode(token)
    {
        /// <summary>
        /// List of array size expressions
        /// </summary>
        public List<ExpressionNode> SizeExpressions { get; set; } = [];
        /// <summary>
        /// List of array items expressions
        /// </summary>
        public List<ExpressionNode> ItemsExpressions { get; set; } = [];

        #region Статика

        /// <summary>
        /// Parse new array instance expression
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="token">Token that represents new keyword</param>
        /// <returns>Parsed new array instance expression</returns>
        public static NewArrayExpressionNode Parse(AstParserStream stream, DSharpToken token)
        {
            stream.Eat(DSharpTokenType.LeftBracket);
            NewArrayExpressionNode expression = new(token);

            ParseExpressions(stream, expression.SizeExpressions, DSharpTokenType.RightBracket, "Required array size expression");

            stream.Eat(DSharpTokenType.RightBracket);

            if (stream.Check(DSharpTokenType.LeftBrace))
            {
                if (expression.SizeExpressions.Count > 0)
                {
                    stream.ThrowPositionException("Items presetting unavailable when array index was indicated");
                }

                stream.Eat(DSharpTokenType.LeftBrace);
                ParseExpressions(stream, expression.ItemsExpressions, DSharpTokenType.RightBrace, "Required array item expression");
                stream.Eat(DSharpTokenType.RightBrace);
            }

            return expression;
        }

        #endregion
    }
}
