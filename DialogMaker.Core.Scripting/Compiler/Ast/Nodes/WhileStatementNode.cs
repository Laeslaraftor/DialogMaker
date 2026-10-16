using DialogMaker.Core.Scripting.Compiler.Lexer;
using System.Collections.Immutable;

namespace DialogMaker.Core.Scripting.Compiler.Ast.Nodes
{
    /// <summary>
    /// While statement node
    /// </summary>
    /// <param name="token">Token that represents while keyword</param>
    public partial class WhileStatementNode(DSharpToken token) : StatementNode(token)
    {
        /// <summary>
        /// Is do-while
        /// </summary>
        public bool FirstDo { get; set; }
        /// <summary>
        /// Condition for execution body
        /// </summary>
        public ExpressionNode? Condition { get; set; }
        /// <summary>
        /// Body that executes while condition is true
        /// </summary>
        public StatementNode? Body { get; set; }

        #region Static

        /// <summary>
        /// Parse while statement starts with current token
        /// </summary>
        /// <param name="stream">Abstract syntax tree parser stream</param>
        /// <returns>Parsed while statement</returns>
        public static WhileStatementNode Parse(AstParserStream stream)
        {
            bool isDo = false;
            DSharpToken whileToken;

            if (stream.Check(DSharpTokenType.Do))
            {
                whileToken = stream.Eat(DSharpTokenType.Do);
                isDo = true;
            }
            else
            {
                whileToken = stream.Eat(DSharpTokenType.While);
            }

            WhileStatementNode statement = new(whileToken)
            {
                FirstDo = true,
            };

            if (isDo)
            {
                statement.Body = ParseCode(stream);
                statement.Body.Parent = statement;
                stream.Eat(DSharpTokenType.While);
                ParseCondition(stream, statement);
                stream.Eat(DSharpTokenType.Semicolon);
            }
            else
            {
                ParseCondition(stream, statement);

                if (!stream.Check(DSharpTokenType.Semicolon))
                {
                    statement.Body = ParseCode(stream);
                    statement.Body.Parent = statement;
                }
                else
                {
                    stream.Eat(DSharpTokenType.Semicolon);
                }
            }


            return statement;
        }

        private static void ParseCondition(AstParserStream stream, WhileStatementNode whileStatement)
        {
            stream.Eat(DSharpTokenType.LeftParen);

            var conditionExpression = ExpressionNode.ParseExpression(stream);
            conditionExpression.Parent = whileStatement;
            whileStatement.Condition = conditionExpression;

            stream.Eat(DSharpTokenType.RightParen);
        }

        #endregion
    }
}
