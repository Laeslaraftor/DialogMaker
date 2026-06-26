using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Text;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Call expression
    /// </summary>
    /// <param name="token">Token that represents calling expression</param>
    public class CallExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Expression that calling
        /// </summary>
        public ExpressionNode? Callee { get; set; }
        /// <summary>
        /// Arguments of calling
        /// </summary>
        public List<ExpressionNode> Arguments { get; set; } = [];

        #region Управление

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            if (Callee == null)
            {
                return base.ToString();
            }

            StringBuilder builder = new();
            builder.AppendLine(base.ToString());
            builder.AppendLine($"Callee: {Callee.ToString().Trim()}");

            if (Arguments.Count > 0)
            {
                builder.AppendLine("Arguments:");
                
                foreach (var arg in Arguments)
                {
                    builder.AppendLine(arg.ToString().Trim());
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Статика

        /// <summary>
        /// Parse argument expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="buffer">Buffer for writing arguments</param>
        /// <param name="openToken">Token that indicate start or arguments</param>
        /// <param name="closeToken">Token that indicate start or arguments</param>
        /// <returns>Arguments open token</returns>
        public static DSharpToken ParseArguments(AstParserStream stream, List<ExpressionNode> buffer, DSharpTokenType openToken = DSharpTokenType.LeftParen, DSharpTokenType closeToken = DSharpTokenType.RightParen)
        {
            var callToken = stream.Eat(openToken);

            while (!stream.Check(closeToken))
            {
                buffer.Add(ParseExpression(stream));

                if (!ArrayExpressionNode.CheckTokenAfterComma(stream, closeToken))
                {
                    stream.ThrowPositionException("Required argument expression");
                }
            }

            stream.Eat(closeToken);

            return callToken;
        }
        /// <summary>
        /// Parse argument expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <param name="openToken">Token that indicate start or arguments</param>
        /// <param name="closeToken">Token that indicate start or arguments</param>
        /// <returns>Parsed arguments</returns>
        public static List<ExpressionNode> ParseArguments(AstParserStream stream, DSharpTokenType openToken = DSharpTokenType.LeftParen, DSharpTokenType closeToken = DSharpTokenType.RightParen)
        {
            List<ExpressionNode> buffer = [];
            ParseArguments(stream, buffer, openToken, closeToken);

            return buffer;
        }

        #endregion
    }
}
