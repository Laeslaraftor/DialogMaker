using DialogMaker.Core.Scripting.Compiler.Lexer;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// Node that represents anonymous delegate
    /// </summary>
    /// <param name="token">Token that represents delegate keyword</param>
    public class DelegateExpressionNode(DSharpToken token) : ExpressionNode(token)
    {
        /// <summary>
        /// Input parameters of delegate
        /// </summary>
        public List<ParameterExpressionNode> Parameters { get; set; } = [];
        /// <summary>
        /// Body of delegate
        /// </summary>
        public BlockStatementNode? Body { get; set; }

        #region Статика

        /// <summary>
        /// Check is current tokens is delegate expression
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Is current tokens is delegate expression</returns>
        public static bool IsDelegate(AstParserStream stream)
        {
            if ((stream.Check(DSharpTokenType.Delegate) && stream.Check(DSharpTokenType.LeftBrace, 1)) ||
                (stream.Check(DSharpTokenType.Delegate) && stream.Check(DSharpTokenType.LeftParen, 1)) ||
                (stream.Check(DSharpTokenType.Identifier) && stream.Check(DSharpTokenType.Lambda, 1)))
            {
                return true;
            }
            if (stream.Check(DSharpTokenType.LeftParen))
            {
                int offset = 1;

                while (!stream.Check(DSharpTokenType.RightParen, offset))
                {
                    offset++;
                }

                if (stream.Check(DSharpTokenType.RightParen, offset) && 
                    stream.Check(DSharpTokenType.Lambda, offset + 1))
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Parse delegate expression starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed delegate expression</returns>
        public static DelegateExpressionNode Parse(AstParserStream stream)
        {
            /*
            delegate
            {
            }
            delegate(p)
            {
            }
            () => {}
            () => expression
            p => {}
            p => expression
            (s, e) => {}
            (s, e) => expression
            */

            DelegateExpressionNode result;
            bool isLambda = false;

            if (stream.Check(DSharpTokenType.Delegate))
            {
                var token = stream.Eat(DSharpTokenType.Delegate);
                result = new(token);

                if (stream.Check(DSharpTokenType.LeftParen))
                {
                    stream.Eat(DSharpTokenType.LeftParen);
                    ParameterExpressionNode.ParseMultiple(stream, result.Parameters, DSharpTokenType.RightParen, false, false);
                    stream.Eat(DSharpTokenType.RightParen);
                }
            }
            else
            {
                isLambda = true;
                List<ParameterExpressionNode> parameters;

                if (stream.Check(DSharpTokenType.LeftParen))
                {
                    stream.Eat(DSharpTokenType.LeftParen);
                    parameters = ParameterExpressionNode.ParseMultiple(stream, DSharpTokenType.RightParen, false, false);
                    stream.Eat(DSharpTokenType.RightParen);
                }
                else
                {
                    parameters = [];
                    var parameter = ParameterExpressionNode.Parse(stream, false, false);
                    parameters.Add(parameter);
                }

                var token = stream.Eat(DSharpTokenType.Lambda);
                result = new(token)
                {
                    Parameters = parameters
                };
            }

            if (!isLambda || stream.Check(DSharpTokenType.LeftBrace))
            {
                result.Body = BlockStatementNode.Parse(stream);
            }
            else
            {
                BlockStatementNode body = new(result.Token);
                BlockStatementNode.ParseBody(stream, body.Statements, DSharpTokenType.Semicolon);

                result.Body = body;
            }

            return result;
        }

        #endregion
    }
}
