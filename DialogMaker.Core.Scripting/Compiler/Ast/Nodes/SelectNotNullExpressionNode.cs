using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents selecting not null expression
    /// </summary>
    /// <param name="token">Token that represents not null selecting operator (??)</param>
    public class SelectNotNullExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Left expression
        /// </summary>
        public ExpressionNode? Left { get; set; }
        /// <summary>
        /// Right expression
        /// </summary>
        public ExpressionNode? Right { get; set; }

        #region Static

        /// <summary>
        /// Parse selecting not null expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="left">Left expression</param>
        /// <returns>Parsed node</returns>
        public static SelectNotNullExpressionNode Parse(AstParserStream stream, ExpressionNode left)
        {
            var token = stream.Eat(DSharpTokenType.IfNull);
            SelectNotNullExpressionNode result = new(token)
            {
                Left = left,
                Right = ParseExpression(stream)
            };

            left.Parent = result;
            result.Right.Parent = result;

            return result;
        }

        #endregion
    }
}
